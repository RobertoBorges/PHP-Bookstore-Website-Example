USE BookstoreDB;
INSERT INTO Users (AzureAdObjectId, Email, UserName, CreatedAt)
VALUES ('00000000-0000-0000-0000-000000000001', 'testuser@local.dev', 'Test User', GETUTCDATE());
