USE RuleSimulation;
GO

------------------------------------------------------------
-- STEP 1: Create staging table
------------------------------------------------------------
IF OBJECT_ID('dbo.Requests_Staging', 'U') IS NOT NULL
    DROP TABLE dbo.Requests_Staging;
GO

CREATE TABLE dbo.Requests_Staging
(
    request_id            varchar(100) NULL,
    submitted_at          varchar(100) NULL,
    channel               varchar(100) NULL,
    region                varchar(100) NULL,
    account_tier          varchar(100) NULL,
    item_code             varchar(100) NULL,
    item_class            varchar(100) NULL,
    quantity              varchar(100) NULL,
    declared_value        varchar(100) NULL,
    item_age_days         varchar(100) NULL,
    has_documentation     varchar(100) NULL,
    prior_requests_90d    varchar(100) NULL,
    flagged_duplicate     varchar(100) NULL,
    recorded_outcome      varchar(100) NULL
);
GO

------------------------------------------------------------
-- STEP 2: Load CSV into staging
------------------------------------------------------------
BULK INSERT dbo.Requests_Staging
FROM 'C:\My Files\Rules-Simulation_SandBox\rule-simulation-app\sql\requests_seed.csv'
WITH
(
    FORMAT = 'CSV',
    FIRSTROW = 2,
    FIELDQUOTE = '"',
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '0x0a',
    CODEPAGE = '65001',
    TABLOCK
);
GO

------------------------------------------------------------
-- STEP 3: Inspect raw outcome values
------------------------------------------------------------
SELECT
    '[' + recorded_outcome + ']' AS raw_outcome,
    LEN(recorded_outcome) AS length_value,
    DATALENGTH(recorded_outcome) AS byte_length
FROM dbo.Requests_Staging
GROUP BY recorded_outcome;
GO

------------------------------------------------------------
-- STEP 4: Clear Requests
------------------------------------------------------------
DELETE FROM dbo.Requests;
GO

------------------------------------------------------------
-- STEP 5: Insert cleaned / converted data
------------------------------------------------------------
INSERT INTO dbo.Requests
(
    request_id,
    submitted_at,
    channel,
    region,
    account_tier,
    item_code,
    item_class,
    quantity,
    declared_value,
    item_age_days,
    has_documentation,
    prior_requests_90d,
    flagged_duplicate,
    recorded_outcome
)
SELECT
    LTRIM(RTRIM(request_id)),

    TRY_CONVERT(
        datetime2(0),
        LTRIM(RTRIM(REPLACE(submitted_at, CHAR(13), ''))),
        127
    ),

    LTRIM(RTRIM(channel)),

    NULLIF(
        LTRIM(RTRIM(REPLACE(region, CHAR(13), ''))),
        ''
    ),

    LTRIM(RTRIM(account_tier)),

    LTRIM(RTRIM(item_code)),

    LTRIM(RTRIM(item_class)),

    TRY_CONVERT(
        int,
        LTRIM(RTRIM(REPLACE(quantity, CHAR(13), '')))
    ),

    TRY_CONVERT(
        decimal(18,2),
        LTRIM(RTRIM(REPLACE(declared_value, CHAR(13), '')))
    ),

    TRY_CONVERT(
        int,
        LTRIM(RTRIM(REPLACE(item_age_days, CHAR(13), '')))
    ),

    CASE
        WHEN LOWER(LTRIM(RTRIM(REPLACE(has_documentation, CHAR(13), '')))) = 'true'
            THEN 1
        WHEN LOWER(LTRIM(RTRIM(REPLACE(has_documentation, CHAR(13), '')))) = 'false'
            THEN 0
        ELSE NULL
    END,

    TRY_CONVERT(
        int,
        LTRIM(RTRIM(REPLACE(prior_requests_90d, CHAR(13), '')))
    ),

    CASE
        WHEN LOWER(LTRIM(RTRIM(REPLACE(flagged_duplicate, CHAR(13), '')))) = 'true'
            THEN 1
        WHEN LOWER(LTRIM(RTRIM(REPLACE(flagged_duplicate, CHAR(13), '')))) = 'false'
            THEN 0
        ELSE NULL
    END,

    LTRIM(
        RTRIM(
            REPLACE(recorded_outcome, CHAR(13), '')
        )
    )

FROM dbo.Requests_Staging;
GO

------------------------------------------------------------
-- STEP 6: Verify count
------------------------------------------------------------
SELECT COUNT(*) AS RequestCount
FROM dbo.Requests;
GO

------------------------------------------------------------
-- STEP 7: Verify outcome distribution
------------------------------------------------------------
SELECT
    recorded_outcome,
    COUNT(*) AS request_count
FROM dbo.Requests
GROUP BY recorded_outcome
ORDER BY recorded_outcome;
GO