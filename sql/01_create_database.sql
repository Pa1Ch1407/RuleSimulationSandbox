IF DB_ID(N'RuleSimulation') IS NULL
BEGIN
    CREATE DATABASE RuleSimulation;
END
GO

USE RuleSimulation;
GO

-- ============================================================
-- Requests
-- ============================================================

IF OBJECT_ID('dbo.Requests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Requests
    (
        request_id            varchar(50)     NOT NULL,
        submitted_at          datetime2(0)    NOT NULL,
        channel               varchar(20)     NOT NULL,
        region                varchar(20)     NULL,
        account_tier          varchar(20)     NOT NULL,
        item_code             varchar(50)     NOT NULL,
        item_class             varchar(20)     NOT NULL,
        quantity              int             NOT NULL,
        declared_value        decimal(18,2)   NOT NULL,
        item_age_days         int             NOT NULL,
        has_documentation     bit             NOT NULL,
        prior_requests_90d    int             NOT NULL,
        flagged_duplicate     bit             NOT NULL,
        recorded_outcome      varchar(30)     NOT NULL,

        CONSTRAINT PK_Requests PRIMARY KEY (request_id),

        CONSTRAINT CK_Requests_Quantity
            CHECK (quantity BETWEEN 1 AND 500),

        CONSTRAINT CK_Requests_ItemAge
            CHECK (item_age_days BETWEEN 0 AND 1200),

        CONSTRAINT CK_Requests_Region
            CHECK (region IS NULL OR region IN ('NA','EMEA','APAC','LATAM')),

        CONSTRAINT CK_Requests_Channel
            CHECK (channel IN ('PORTAL','EMAIL','API','PHONE')),

        CONSTRAINT CK_Requests_Tier
            CHECK (account_tier IN ('BRONZE','SILVER','GOLD','PLATINUM')),

        CONSTRAINT CK_Requests_ItemClass
            CHECK (item_class IN ('CLASS_A','CLASS_B','CLASS_C','CLASS_D')),

        CONSTRAINT CK_Requests_Outcome
            CHECK (
                recorded_outcome IN
                (
                    'AUTO_APPROVED',
                    'AUTO_DECLINED',
                    'MANUAL_APPROVED',
                    'MANUAL_DECLINED',
                    'ROUTED_SPECIALIST'
                )
            )
    );

    CREATE INDEX IX_Requests_SubmittedAt
        ON dbo.Requests(submitted_at);

    CREATE INDEX IX_Requests_Region_Tier
        ON dbo.Requests(region, account_tier);
END
GO


-- ============================================================
-- RuleSet
-- ============================================================

IF OBJECT_ID('dbo.RuleSet', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleSet
    (
        id               bigint IDENTITY(1,1) NOT NULL,
        name             nvarchar(200) NOT NULL,
        enabled          bit NOT NULL
            CONSTRAINT DF_RuleSet_Enabled DEFAULT(1),
        created_at_utc   datetime2(0) NOT NULL,
        updated_at_utc   datetime2(0) NOT NULL,

        CONSTRAINT PK_RuleSet PRIMARY KEY (id)
    );
END
GO


-- ============================================================
-- Rule
-- ============================================================

IF OBJECT_ID('dbo.Rule', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.[Rule]
    (
        id                bigint IDENTITY(1,1) NOT NULL,
        rule_set_id       bigint NOT NULL,
        name              nvarchar(200) NOT NULL,
        priority          int NOT NULL,
        enabled           bit NOT NULL
            CONSTRAINT DF_Rule_Enabled DEFAULT(1),
        condition_join    varchar(3) NOT NULL,
        action            varchar(30) NOT NULL,
        display_order     int NOT NULL,

        CONSTRAINT PK_Rule PRIMARY KEY (id),

        CONSTRAINT FK_Rule_RuleSet
            FOREIGN KEY(rule_set_id)
            REFERENCES dbo.RuleSet(id)
            ON DELETE CASCADE,

        CONSTRAINT CK_Rule_ConditionJoin
            CHECK (condition_join IN ('AND','OR')),

        CONSTRAINT CK_Rule_Action
            CHECK (
                action IN
                (
                    'AUTO_APPROVE',
                    'AUTO_DECLINE',
                    'ROUTE_SPECIALIST',
                    'MANUAL_REVIEW'
                )
            ),

        CONSTRAINT UQ_Rule_RuleSetDisplayOrder
            UNIQUE(rule_set_id, display_order)
    );

    CREATE INDEX IX_Rule_RuleSet_Priority
        ON dbo.[Rule]
        (
            rule_set_id,
            enabled,
            priority DESC,
            display_order ASC,
            id ASC
        );
END
GO


-- ============================================================
-- RuleCondition
-- ============================================================

IF OBJECT_ID('dbo.RuleCondition', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RuleCondition
    (
        id             bigint IDENTITY(1,1) NOT NULL,
        rule_id        bigint NOT NULL,
        sequence       int NOT NULL,
        field          varchar(50) NOT NULL,
        operator       varchar(30) NOT NULL,
        value          nvarchar(4000) NULL,
        values_json    nvarchar(4000) NULL,

        CONSTRAINT PK_RuleCondition PRIMARY KEY (id),

        CONSTRAINT FK_RuleCondition_Rule
            FOREIGN KEY(rule_id)
            REFERENCES dbo.[Rule](id)
            ON DELETE CASCADE,

        CONSTRAINT UQ_RuleCondition_RuleSequence
            UNIQUE(rule_id, sequence)
    );

    CREATE INDEX IX_RuleCondition_RuleId
        ON dbo.RuleCondition(rule_id, sequence);
END
GO