# Project Submission Evidence & Verification Checklist

> [!IMPORTANT]
> **Privacy Notice**: Please ensure that you remove or sanitize all AWS account identifiers (account IDs, IAM access keys, private bucket names) before submitting your final report or screenshots.

---

## 1. Storage Verification Evidence

### Object Keys & Partitioning
Document uploads are partitioned using the date and GUID pattern:
`{KeyPrefix}/{yyyy}/{MM}/{dd}/{guid:N}{.ext}`

- [ ] **S3 Object Key Example:**
  ```text
  documents/uploads/2026/10/16/9f4a1c5d8e7b42019876543210abcdef.txt
  ```

### User Metadata Headers on S3
Verify that the following metadata headers are present on uploaded S3 objects:
- [ ] `original-filename`: `<filename.ext>`
- [ ] `content-type`: `<mime-type>`
- [ ] `uploaded-utc`: `<ISO 8601 UTC timestamp>`
- [ ] `content-length`: `<size in bytes>`

**AWS CLI Verification Command:**
```bash
aws s3api head-object \
  --bucket <your-bucket-name> \
  --key "documents/uploads/2026/10/16/<your-object-key>.txt"
```

---

## 2. CI/CD Run URLs & Status

Provide links or run logs for both GitHub Actions workflows:

| Workflow | Purpose | Status | Run URL / Identifier |
| :--- | :--- | :--- | :--- |
| **CI** (`ci.yml`) | Pull Request & Branch Validation (Zero AWS Credentials) | Passed | `https://github.com/<owner>/<repo>/actions/runs/<id>` |
| **Deploy** (`deploy.yml`) | Build, Test, OIDC Authenticate, & Deploy to Elastic Beanstalk | Passed | `https://github.com/<owner>/<repo>/actions/runs/<id>` |

---

## 3. Security & IAM Analysis

### Zero-Credential CI Runs
- [ ] Confirmed that pull request workflows execute without AWS credentials or secrets.
- [ ] Test doubles (`RecordingS3ObjectClientSpy`, `FakeS3ObjectClient`) simulate S3 interactions deterministically.

### IAM Least Privilege & Denial Analysis
- [ ] **OIDC Role Trust Policy**: Scoped strictly to repository (`repo:<org>/<repo>:ref:refs/heads/main`).
- [ ] **S3 Bucket Policy**: Limited to `s3:PutObject` on the target prefix (`documents/uploads/*`).
- [ ] **IAM Denial Simulation**: Verify that attempting unauthorized actions (e.g., `s3:DeleteBucket` or writing outside the prefix) produces an explicit `AccessDenied` error from AWS IAM.

---

## 4. Demonstration & Video Recording

- [ ] Video recorded demonstrating:
  - **GET**: Data table loads initial items with loading indicator and telemetry log.
  - **POST**: File upload and JSON text document creation with S3 key generation.
  - **PUT**: Updating title and description with immediate table reflection.
  - **DELETE**: Deleting item with modal confirmation and empty state handling.
  - **Error Handling**: Pointing to invalid port demonstrates explicit error alert banner.
- [ ] Video file or streaming link provided: `<insert link>`

---

## 5. Post-Evaluation Cleanup Confirmation

- [ ] All temporary uploaded test objects removed from S3 bucket:
  ```bash
  aws s3 rm s3://<your-bucket-name>/documents/uploads/ --recursive
  ```
- [ ] AWS Elastic Beanstalk environment terminated or scaled down if required:
  ```bash
  aws elasticbeanstalk terminate-environment --environment-name wp1-fall26-aws-dev
  ```
