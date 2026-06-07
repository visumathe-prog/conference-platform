provider "aws" {
  region = "eu-central-1"
}

resource "aws_s3_bucket" "certificates" {
  bucket = "conference-certificates-${random_id.suffix.hex}"
}

resource "random_id" "suffix" {
  byte_length = 4
}

resource "aws_db_instance" "postgres" {
  identifier     = "conference-db"
  engine         = "postgres"
  instance_class = "db.t3.micro"
  allocated_storage = 20
  username       = "admin"
  password       = var.db_password
  skip_final_snapshot = true
}

variable "db_password" {
  description = "Database password"
  sensitive   = true
}

output "s3_bucket" {
  value = aws_s3_bucket.certificates.bucket
}
