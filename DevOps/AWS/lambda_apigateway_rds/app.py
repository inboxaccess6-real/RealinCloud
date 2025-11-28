#!/usr/bin/env python3
import aws_cdk as cdk
from lambda_api_stack import LambdaApiStack

app = cdk.App()

LambdaApiStack(
    app,
    "RealEstateLambdaStack",
    env=cdk.Environment(region="ap-south-1")
)

app.synth()