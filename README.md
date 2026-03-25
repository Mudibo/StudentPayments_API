# Student Payments Integration API

## Project Overview
This API allows seamless integration between banks and universities for student payment notification processing. It enables banks to validate students and send payment notification information to universities, allowing universities to reconcile payments made by students at the bank.

## Features
- Student registration onto the database.
- Student Validation of enrollment status and existence.
- Admin management of bank clients (Client ID / Client Secret)
- Machine-to-machine (M2M) authentication for banks using OAuth 2.0
- Admin management of student dues.
- Payment notification addition for student payments.
- Dockerized deployment for easy setup

## Getting Started

### Prerequisites
- Docker

## Setup Instructions
1. Clone the repository:
    ```bash
   git clone https://github.com/Mudibo/StudentPayments_API.git
   cd STUDENTPAYMENTS_API
2. Configure environment variables (see below). Replace each variable with the actual value.

## Environment Variables & Configuration
- ASPNETCORE_ENVIRONMENT=Production
- ConnectionStrings__DefaultConnection=${ConnectionStrings__DefaultConnection}
- Jwt__Secret=${Jwt__Secret}
- Jwt__Issuer=${Jwt__Issuer}
- Jwt__Audience=${Jwt__Audience}
- Jwt__TokenLifetimeMinutes=${Jwt__TokenLifetimeMinutes}
- ConnectionStrings__Redis=${ConnectionStrings__Redis}

## Running the Application
1. App Setup
    ```bash
    docker-compose up --build

## Authentication
1. Admin Authentication
Admins log in to manage bank clients and student dues.

2. Bank M2M Authentication
Banks authenticate using their ClientId, ClientSecret, grant_type, and scope to obtain a token.

## Usage Workflow
1. Student Registration: Students register on the platform
2. Admin logs in and adds bank clients, specifying their ClientId, ClientSecret, and allowed scopes for policy-based access control.
3. Admin add dues for students
4. Bank authenticates via M2M to obtain a token.
5. Student Validation: Bank Validates student before payment.
6. Payment Processing: Student pays at the bank.
7. Payment Notification: Bank sends payment notification information to the university via the API.



