1. base request handler
2. validation wrong place
3. CreatePasswordCommandHandler - don't split keyConfig
4. PasswordDbContext - OnModelCreating is not needed
5. filter registration should be global, it would be easier
6. CreatePassword - still Exception returns BadRequest
7. Split unit tests classes to test only single class
8. integration tests so pure
9. wrong API routes
10. mess between MediatR and API Response - use different models
11. location of created password is wrong
12. don't forget namespaces
