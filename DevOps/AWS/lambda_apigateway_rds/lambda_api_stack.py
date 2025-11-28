from aws_cdk import (
    Stack,
    Duration,
    RemovalPolicy,
    aws_ec2 as ec2,
    aws_lambda as lambda_,
    aws_apigatewayv2 as apigw,
    aws_apigatewayv2_integrations as integrations,
    aws_apigatewayv2_authorizers as authorizers,
    aws_iam as iam,
    aws_rds as rds,
    aws_secretsmanager as secrets,
    aws_opensearchservice as opensearch,
)
from constructs import Construct


class LambdaApiStack(Stack):

    def __init__(self, scope: Construct, id: str, **kwargs):
        super().__init__(scope, id, **kwargs)

        # -----------------------------
        # 1. VPC
        # -----------------------------
        vpc = ec2.Vpc(self, "ApiVpc", max_azs=2, nat_gateways=1)

        # -----------------------------
        # 2. RDS Postgres
        # -----------------------------
        db_secret = rds.DatabaseSecret(
            self, "DbSecret", username="postgres"
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
            instance_type=ec2.InstanceType("t3.micro"),
            allocated_storage=20,
            credentials=rds.Credentials.from_secret(db_secret),
            multi_az=False,
            publicly_accessible=False,
            deletion_protection=False,
            removal_policy=RemovalPolicy.DESTROY,
        )

        # -----------------------------
        # 3. RDS Proxy
        # -----------------------------
        proxy = rds.DatabaseProxy(
            self,
            "DbProxy",
            proxy_target=rds.ProxyTarget.from_instance(db_instance),
            secrets=[db_secret],
            vpc=vpc,
            require_tls=False,
        )

        # -----------------------------
        # 4. OpenSearch Domain
        # -----------------------------
        opensearch_domain = opensearch.Domain(
            self,
            "SearchDomain",
            version=opensearch.EngineVersion.OPENSEARCH_2_13,
            capacity=opensearch.CapacityConfig(
                data_nodes=1,
                data_node_instance_type="t3.small.search",
            ),
            ebs=opensearch.EbsOptions(volume_size=10),
            removal_policy=RemovalPolicy.DESTROY,
        )

        # -----------------------------
        # 5. JWT Secret
        # -----------------------------
        jwt_secret = secrets.Secret(
            self,
            "JwtSecret",
            generate_secret_string=secrets.SecretStringGenerator(
                password_length=64
            )
        )

        common_env = {
            "DB_PROXY_HOST": proxy.endpoint,
            "DB_SECRET_ARN": db_secret.secret_arn,
            "JWT_SECRET_ARN": jwt_secret.secret_arn,
            "OPENSEARCH_ENDPOINT": opensearch_domain.domain_endpoint,
        }

        # -----------------------------
        # 6. Auth Lambda
        # -----------------------------
        auth_lambda = lambda_.Function(
            self,
            "AuthLambda",
            runtime=lambda_.Runtime.PROVIDED_AL2023,
            code=lambda_.Code.from_asset("swift-auth-lambda/"),
            handler="bootstrap",
            memory_size=1024,
            timeout=Duration.seconds(10),
            environment=common_env,
        )
        jwt_secret.grant_read(auth_lambda)

        # -----------------------------
        # 7. API Lambda
        # -----------------------------
        api_lambda = lambda_.Function(
            self,
            "ApiLambda",
            runtime=lambda_.Runtime.PROVIDED_AL2023,
            code=lambda_.Code.from_asset("swift-api-lambda/"),
            handler="bootstrap",
            vpc=vpc,
            memory_size=1024,
            timeout=Duration.seconds(15),
            environment=common_env,
        )
        db_secret.grant_read(api_lambda)
        proxy.grant_connect(api_lambda)
        opensearch_domain.grant_write(api_lambda)

        # -----------------------------
        # 8. Sync Lambda
        # -----------------------------
        sync_lambda = lambda_.Function(
            self,
            "SyncLambda",
            runtime=lambda_.Runtime.PROVIDED_AL2023,
            code=lambda_.Code.from_asset("swift-sync-lambda/"),
            handler="bootstrap",
            vpc=vpc,
            memory_size=1024,
            timeout=Duration.seconds(10),
            environment=common_env,
        )
        proxy.grant_connect(sync_lambda)
        db_secret.grant_read(sync_lambda)
        opensearch_domain.grant_write(sync_lambda)

        # -----------------------------
        # 9. JWT Lambda Authorizer (NEW)
        # -----------------------------
        jwt_authorizer_lambda = lambda_.Function(
            self,
            "JwtAuthorizerLambda",
            runtime=lambda_.Runtime.PROVIDED_AL2023,
            code=lambda_.Code.from_asset("swift-authorizer-lambda/"),
            handler="bootstrap",
            timeout=Duration.seconds(5),
            memory_size=512,
            environment={
                "JWT_SECRET_ARN": jwt_secret.secret_arn
            }
        )
        jwt_secret.grant_read(jwt_authorizer_lambda)

        authorizer = authorizers.HttpLambdaAuthorizer(
            "JwtAuthorizer",
            jwt_authorizer_lambda,
            response_types=[authorizers.HttpLambdaResponseType.IAM],
        )

        # -----------------------------
        # 10. API Gateway (HTTP API)
        # -----------------------------
        http_api = apigw.HttpApi(self, "RealEstateHttpApi")

        # /auth (NO AUTHORIZER)
        http_api.add_routes(
            path="/auth",
            methods=[apigw.HttpMethod.POST],
            integration=integrations.LambdaProxyIntegration(
                handler=auth_lambda
            )
        )

        # /properties (PROTECTED)
        http_api.add_routes(
            path="/properties",
            methods=[apigw.HttpMethod.GET, apigw.HttpMethod.POST],
            integration=integrations.LambdaProxyIntegration(
                handler=api_lambda
            ),
            authorizer=authorizer,
        )

        # /sync (PROTECTED)
        http_api.add_routes(
            path="/sync",
            methods=[apigw.HttpMethod.POST],
            integration=integrations.LambdaProxyIntegration(
                handler=sync_lambda
            ),
            authorizer=authorizer,
        )

        # Output
        self.http_api_url = self.node.try_get_context("api_url")