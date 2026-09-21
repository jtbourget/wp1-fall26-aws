# Wp1Fall26Aws

## Storage Behavior
Files are uploaded via POST /documents.

## Validation Policy
- Maximum size: 1 MiB
- Allowed extensions: .txt, .pdf, .png, .jpg, .jpeg
- Allowed content types: text/plain, application/pdf, image/png, image/jpeg
- Empty files are rejected.

## Key Format
{KeyPrefix}/{yyyy}/{MM}/{dd}/{guid:N}{.ext}

## Configuration Keys
- S3Storage:BucketName
- S3Storage:KeyPrefix
- S3Storage:Region
- S3Storage:MaxFileSizeBytes

## Run Locally
Use user-secrets to configure the bucket name:
`ash
dotnet user-secrets --project src/Wp1Fall26Aws.Api set "S3Storage:BucketName" "<bucket>"
dotnet run --project src/Wp1Fall26Aws.Api
`
"@ | Out-File -FilePath "C:\Users\joshu\source\repos\wp1-fall26-aws\README.md" -Encoding utf8

@"
# Evidence

Please ensure that you remove all account identifiers before submitting!

- [ ] Object keys and metadata
- [ ] CI run URLs
- [ ] IAM denial analysis
- [ ] Cleanup confirmation
