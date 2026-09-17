CREATE TABLE UserRefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId VARCHAR(50) NOT NULL,
    Token VARCHAR(200) NOT NULL,
    ExpiresUtc DATETIME NOT NULL,
    CreatedUtc DATETIME NOT NULL DEFAULT GETUTCDATE(),
    RevokedUtc DATETIME NULL,
    ReplacedByToken VARCHAR(200) NULL
);

CREATE NONCLUSTERED INDEX IX_UserRefreshTokens_Token ON UserRefreshTokens(Token);
CREATE NONCLUSTERED INDEX IX_UserRefreshTokens_UserId ON UserRefreshTokens(UserId);