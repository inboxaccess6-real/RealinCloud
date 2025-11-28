from aws_cdk import (
    Stack,
    Duration,
    aws_ec2 as ec2,
    aws_ecs as ecs,
    aws_ecr as ecr,
    aws_rds as rds,
    aws_ecs_patterns as ecs_patterns,
    aws_route53 as route53,
    aws_certificatemanager as acm,
    aws_logs as logs,
    aws_secretsmanager as secretsmanager,
)
from constructs import Construct

class RealinEcsFargateStack(Stack):

    def __init__(self, scope: Construct, id: str, **kwargs) -> None:
        super().__init__(scope, id, **kwargs)

        # ----------------------------
        # 1. VPC (Public + Private Subnets)
        # ----------------------------
        vpc = ec2.Vpc(
            self,
            "MyVpc",
            max_azs=2,
            nat_gateways=1,
        )

        # ----------------------------
        # 2. ECR Repo (your Swift image pushed via GitHub Actions)
        # ----------------------------
        repo = ecr.Repository.from_repository_name(
            self,
            "SwiftAPIRepo",
            repository_name="swift-api"
        )

        # ----------------------------
        # 3. ECS Cluster
        # ----------------------------
        cluster = ecs.Cluster(
            self,
            "EcsCluster",
            vpc=vpc
        )

        # ----------------------------
        # 4. Route53 Hosted Zone
        # ----------------------------
        hosted_zone = route53.HostedZone.from_lookup(
            self,
            "HostedZone",
            domain_name="yourdomain.com"    # <<< change this
        )

        # ----------------------------
        # 5. SSL Certificate
        # ----------------------------
        certificate = acm.DnsValidatedCertificate(
            self,
            "ApiCert",
            hosted_zone=hosted_zone,
            domain_name="api.yourdomain.com",  # <<< change this
            region=self.region,
        )

        # ----------------------------
        # 6. RDS PostgreSQL
        # ----------------------------
        db_security_group = ec2.SecurityGroup(
            self,
            "DbSG",
            vpc=vpc,
            description="Allow ECS to access Postgres",
            allow_all_outbound=True
        )

        db_instance = rds.DatabaseInstance(
            self,
            "PostgresDB",
            engine=rds.DatabaseInstanceEngine.postgres(
                version=rds.PostgresEngineVersion.VER_15
            ),
            vpc=vpc,
            vpc_subnets=ec2.SubnetSelection(
                subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS
            ),
            security_groups=[db_security_group],
            allocated_storage=20,
            instance_type=ec2.InstanceType("t3.micro"),
            multi_az=False,
            publicly_accessible=False,
            deletion_protection=False,
            removal_policy=rds.RemovalPolicy.DESTROY,
            database_name="mydb",
            credentials=rds.Credentials.from_generated_secret("postgres"),
        )

        # ----------------------------
        # 7. ECS Fargate Service + ALB (HTTPS)
        # ----------------------------
        fargate_service = ecs_patterns.ApplicationLoadBalancedFargateService(
            self,
            "SwiftApiService",
            cluster=cluster,
            cpu=512,
            memory_limit_mib=1024,
            desired_count=1,
            listener_port=443,
            certificate=certificate,
            domain_name="api.yourdomain.com",  # <<< change this
            domain_zone=hosted_zone,
            task_image_options=ecs_patterns.ApplicationLoadBalancedTaskImageOptions(
                image=ecs.ContainerImage.from_ecr_repository(
                    repository=repo,
                    tag="latest"  # your pushed tag
                ),
                container_port=8080,
                environment={
                    "DATABASE_HOST": db_instance.db_instance_endpoint_address,
                    "DATABASE_PORT": "5432",
                    "DATABASE_USER": "postgres",
                    "DATABASE_NAME": "mydb",
                },
                secrets={
                    "DATABASE_PASSWORD": ecs.Secret.from_secrets_manager(
                        db_instance.secret,
                        field="password"
                    )
                },
                log_driver=ecs.LogDrivers.aws_logs(
                    stream_prefix="swift-api",
                    log_retention=logs.RetentionDays.ONE_WEEK
                )
            ),
            public_load_balancer=True,
        )

        # ----------------------------
        # 8. Security Group Rules
        # ----------------------------
        # ALB → ECS
        fargate_service.service.connections.allow_from(
            fargate_service.load_balancer,
            ec2.Port.tcp(8080),
            "Allow ALB to reach ECS"
        )

        # ECS → RDS
        db_security_group.add_ingress_rule(
            peer=fargate_service.service.connections.security_groups[0],
            connection=ec2.Port.tcp(5432),
            description="Allow ECS tasks to reach RDS"
        )

        # ----------------------------
        # Output URL
        # ----------------------------
        self.output = fargate_service.load_balancer.load_balancer_dns_name