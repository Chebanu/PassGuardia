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
- body: `{ password: string }`

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

#### Password Not Found

- status: `404`
- response: `{ errors: string[] }`

### Common Responses

#### Server Error

- status: `500`
- response: `{ errors: string[] }`
