Client App (iOS / Android / macOS)
        |
        | (login request: apple/google/otp)
        v
+----------------------+
|   Auth Lambda        |
|  - Apple token verify|
|  - Google token verify|
|  - OTP verify        |
|  - Issue JWT + RT    |
+----------------------+
        |
        | (returns JWT & Refresh Token)
        v
Client stores tokens
        |
        | (subsequent API requests)
        |   Authorization: Bearer <access_token>
        v
+--------------------------+
|  API Gateway             |
|    |
|    +--> Lambda Authorizer <-- verifies JWT (fast)
|            |
|            +--> returns IAM policy 
|                 with user_id context
+--------------------------+
        |
        v
+--------------------------+
|       Main API Lambda    |
|  Receives user_id, etc.  |
+--------------------------+