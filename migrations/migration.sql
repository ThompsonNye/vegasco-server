CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240803173803_AddCarsAndUsers') THEN
    CREATE TABLE "Users" (
        "Id" text NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240803173803_AddCarsAndUsers') THEN
    CREATE TABLE "Cars" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "UserId" text NOT NULL,
        CONSTRAINT "PK_Cars" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Cars_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240803173803_AddCarsAndUsers') THEN
    CREATE INDEX "IX_Cars_UserId" ON "Cars" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240803173803_AddCarsAndUsers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240803173803_AddCarsAndUsers', '8.0.7');
    END IF;
END $EF$;
COMMIT;

