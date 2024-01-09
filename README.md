# PassGuardia

PassGuardia is a HTTP API for password management. With PassGuardia you can store your passwords in a secure way and share them.

## Functional Requirement

- user will be able to store his password securely and receive a unique link to access it
- user will be able to share his password with other users
- user is able to restore his password with a unique link received below
- user will be able to create private and public passwords
- password owners are able to revoke access to their passwords and change visibility
- all requests/responses should be audited in a secure way (without showing the password)
- only signed-in users are able to create passwords
- private passwords are only visible to the owner and the users he shared it with
- public passwords are visible to all users (even unauthorized ones)
- valid password should be non-empty string and max 100 characters
- there are two user roles: `admin` and `user`
- Admin is able to see audit logs
- Username length from 5 to 20 characters. English letters, numbers, and underscore are allowed.
- Password length from 8 to 100 characters. English letters, numbers, special characters and underscore are allowed.

## Non-Functional Requirement

- all passwords should be encrypted (use AES algorithm)
- use PostgreSQL as a database
- authentication should be done with JWT in Header
- unit/integration tests should be written
- caching layer should be present
- logging should be present

## Database Structure

- passwords table:
  - identifier (guid) - primary key
  - password (string) - encrypted

## API

### Create password

- method: `POST`
- path: `/passwords`
- body: `{ password: string, visibility: enum { public, private } }`

#### Password Created

- status: `201`
- headers: `{ Location: string }`
- response: `{ passwordId: guid }`

#### Validation Error Response

- status: `400`
- response: `{ errors: string[] }`

### Get password

- method: `GET`
- path: `/passwords/{passwordId}`

#### Password Found

- status: `200`
- response: `{ password: string }`

#### Password Not Found Or Forbidden To Access

- status: `400`
- response: `{ errors: string[] }`

### Update password

- method: `PUT`
- path: `/passwords/{passwordId}`
- body: `{ visibility: enum { public, private } }`

#### Password Updated

- status: `204`

#### Password To Update Not Found Or Forbidden To Access

- status: `400`
- response: `{ errors: string[] }`

#### Update Password Validation Error Response

- status: `400`
- response: `{ errors: string[] }`

### Register user

- method: `POST`
- path: `/users`

#### User Registered

- status: `201`
- response: `{ userId: guid }`

#### Username Taken

- status: `400`
- response: `{ errors: string[] }`

### Authenticate user

- method: `POST`
- path: `/users/authenticate`

#### User Authenticated

- status: `200`
- response: `{ token: string }`

#### User Not Found or Invalid Password

- status: `400`
- response: `{ errors: string[] }`

### Get audit

- method: `GET`
- path: `/audit?pageNumber={pageNumber}&pageSize={pageSize}`
- notes: `pageNumber and pageSize are optional (defaults are 1 and 100) and positive integers. Page size should be less than 1000.`

#### Pagination Error

- status: `400`
- response: `{ errors: string[] }`

#### Audit Found

- status: `200`
- response: `{ audit: { id: string, requestPath: string, requestMethod: string, exception: string, statusCode: int, timestamp: string }[] }`

### Common Responses

#### Server Error

- status: `500`
- response: `{ errors: string[] }`
