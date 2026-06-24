# Building with AWS CodeBuild and Publishing to CodeArtifact

This repository contains a .NET solution for `EdFi.Db.Deploy`. The following guide shows how to build the project in AWS CodeBuild and publish the resulting NuGet package(s) into AWS CodeArtifact.

Files added:
- `buildspec.yml` — CodeBuild spec to build, pack, and push NuGet packages to CodeArtifact with CI-driven versioning and PackageId override (no changes to the original csproj).

---

## Package Details
- **PackageId**: `EdFi.Suite3.Db.Deploy` — this is the NuGet package identifier published to CodeArtifact. The buildspec overrides the original PackageId at pack time via `-p:PackageId` (original csproj remains unchanged).
- **Versioning**: Versions are determined at build time via the `PACKAGE_VERSION` environment variable or derived from git tags/CodeBuild build numbers (see below).

---

## Required AWS Resources and Permissions
- A CodeArtifact domain and repository (NuGet format).
- A CodeBuild project (Linux) with a managed image that has .NET 8 installed (or provide a custom image).
  - The build image must include AWS CLI v2 and the .NET 8 SDK.
  - Example: create a custom CodeBuild image based on `mcr.microsoft.com/dotnet/sdk:8.0` and publish it to ECR.
- The CodeBuild service role must have permissions at minimum to:
  - `codeartifact:GetAuthorizationToken`
  - `codeartifact:GetRepositoryEndpoint`
  - `codeartifact:ReadFromRepository`
  - `codeartifact:PublishPackageVersion`
  - `sts:GetServiceBearerToken` (required for CodeArtifact authentication)
  - `s3:GetObject` / `PutObject` (if you use S3 for artifacts)
  - `logs:CreateLogGroup`, `logs:CreateLogStream`, `logs:PutLogEvents`

Example least-privilege IAM policy:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "codeartifact:GetAuthorizationToken",
        "codeartifact:GetRepositoryEndpoint",
        "codeartifact:ReadFromRepository",
        "codeartifact:PublishPackageVersion"
      ],
      "Resource": [
        "arn:aws:codeartifact:<region>:<account>:domain/<domain>",
        "arn:aws:codeartifact:<region>:<account>:repository/<domain>/<repository>"
      ]
    },
    {
      "Effect": "Allow",
      "Action": "sts:GetServiceBearerToken",
      "Resource": "*",
      "Condition": {
        "StringEquals": {
          "sts:AWSServiceName": "codeartifact.amazonaws.com"
        }
      }
    }
  ]
}
```

---

## Environment Variables

Set these in the CodeBuild project environment (or via the console/CLI):

| Variable | Purpose | Example |
|----------|---------|---------|
| `CODEARTIFACT_DOMAIN` | CodeArtifact domain name | `doubleline` |
| `CODEARTIFACT_DOMAIN_OWNER` | AWS account ID that owns the domain | `194840617583` |
| `CODEARTIFACT_REPOSITORY` | CodeArtifact NuGet repository | `MiDataHub` |
| `AWS_DEFAULT_REGION` | AWS region | `us-west-2` |
| `PACKAGE_VERSION` | (Optional) Package version; if not set, derived from git tag or build number | `4.1.0` or empty |

---

## CI-Driven Versioning

The `buildspec.yml` supports three approaches to versioning:

1. **Environment variable** (recommended for CI/CD):
   - Set `PACKAGE_VERSION` in the CodeBuild project environment.
   - Example: `PACKAGE_VERSION=4.1.0` produces package `EdFi.Suite3.Db.Deploy.4.1.0.nupkg`.

2. **Git tag** (recommended for release branches):
   - Push a git tag (e.g., `git tag 4.1.0 && git push origin 4.1.0`).
   - If `PACKAGE_VERSION` is not set and a git tag exists, the buildspec uses the tag as the version.

3. **CodeBuild build number** (fallback):
   - If neither `PACKAGE_VERSION` nor a git tag is available, the buildspec uses `0.0.${CODEBUILD_BUILD_NUMBER}`.
   - Example: build #123 produces version `0.0.123`.

---

## Setup Instructions

### 1. Create a CodeArtifact Domain and Repository
```bash
aws codeartifact create-domain --domain doubleline --region us-west-2
aws codeartifact create-repository   --domain doubleline   --repository MiDataHub   --format nuget   --region us-west-2
```

### 2. Create or Update a CodeBuild Project
```bash
aws codebuild create-project   --name EdFi.Db.Deploy-build   --source type=GITHUB,location=https://github.com/your-org/Ed-Fi-Databases.git,buildspec=.aws/buildspec.yml   --artifacts type=CODEPIPELINE   --service-role arn:aws:iam::194840617583:role/CodeBuildServiceRole   --environment type=LINUX_CONTAINER,image=aws/codebuild/standard:8.0,computeType=BUILD_GENERAL1_SMALL   --region us-west-2
```

Set environment variables:
- `CODEARTIFACT_DOMAIN=doubleline`
- `CODEARTIFACT_DOMAIN_OWNER=194840617583`
- `CODEARTIFACT_REPOSITORY=MiDataHub`
- `AWS_DEFAULT_REGION=us-west-2`
- `PACKAGE_VERSION` (optional)

### 3. Run the Build
The buildspec will:
1. Install AWS CodeArtifact NuGet credential provider.
2. Restore and build the solution.
3. Compute package version.
4. Pack the project.
5. Login to CodeArtifact for restore.
6. Push `.nupkg` files to CodeArtifact.

---

## Local Publishing Guide
```bash
# Login to CodeArtifact
aws codeartifact login --tool dotnet --repository MiDataHub --domain doubleline --domain-owner 194840617583 --region us-west-2

# Get token
export CODEARTIFACT_AUTH_TOKEN=$(aws codeartifact get-authorization-token --domain doubleline --domain-owner 194840617583 --region us-west-2 --query authorizationToken --output text)

# Pack
dotnet pack src/EdFi.Db.Deploy/EdFi.Db.Deploy.csproj -c Release -o ./artifacts -p:PackageVersion=4.1.0 -p:PackageId=EdFi.Suite3.Db.Deploy

# Push
dotnet nuget push ./artifacts/*.nupkg --source doubleline/MiDataHub --api-key $CODEARTIFACT_AUTH_TOKEN --skip-duplicate
```

---

## Important Notes
- Tokens expire after 12 hours.
- Repository names are case-sensitive.
- `aws codeartifact login` adds source as `<domain>/<repository>` in NuGet.Config.
- Ensure AWS CLI v2 and .NET 8 SDK are installed.
