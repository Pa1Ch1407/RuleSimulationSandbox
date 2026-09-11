namespace RuleSimulation.Api.Domain;

public enum Channel { PORTAL, EMAIL, API, PHONE }
public enum Region { NA, EMEA, APAC, LATAM }
public enum AccountTier { BRONZE, SILVER, GOLD, PLATINUM }
public enum ItemClass { CLASS_A, CLASS_B, CLASS_C, CLASS_D }
public enum RecordedOutcome { AUTO_APPROVED, AUTO_DECLINED, MANUAL_APPROVED, MANUAL_DECLINED, ROUTED_SPECIALIST }
public enum RuleAction { AUTO_APPROVE, AUTO_DECLINE, ROUTE_SPECIALIST, MANUAL_REVIEW }
public enum ConditionJoin { AND, OR }
public enum ConditionOperator { EQ, NEQ, GT, GTE, LT, LTE, EQUALS, NOT_EQUALS, IN, NOT_IN, IS_TRUE, IS_FALSE }
public enum DecisionFamily { AUTO_APPROVE, AUTO_DECLINE, ROUTE_SPECIALIST, MANUAL }
