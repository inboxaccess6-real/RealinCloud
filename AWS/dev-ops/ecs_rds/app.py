#!/usr/bin/env python3
import aws_cdk as cdk
from ecs_stack import RealinEcsFargateStack

app = cdk.App()

RealinEcsFargateStack(
    app,
    "RealinApiEcsStack",
    env=cdk.Environment(region="ap-south-1")  # Change if needed
)

app.synth()