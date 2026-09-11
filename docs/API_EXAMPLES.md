# API examples

## Create a rule set

```http
POST /api/rule-sets
Content-Type: application/json
```

```json
{
  "name": "Duplicate and high value rules",
  "enabled": true,
  "rules": [
    {
      "name": "Duplicate request",
      "priority": 100,
      "enabled": true,
      "conditionJoin": "AND",
      "action": "AUTO_DECLINE",
      "displayOrder": 1,
      "conditions": [
        {
          "field": "flagged_duplicate",
          "operator": "IS_TRUE",
          "value": null,
          "values": null
        }
      ]
    },
    {
      "name": "High value",
      "priority": 50,
      "enabled": true,
      "conditionJoin": "AND",
      "action": "ROUTE_SPECIALIST",
      "displayOrder": 2,
      "conditions": [
        {
          "field": "declared_value",
          "operator": "GTE",
          "value": "10000",
          "values": null
        }
      ]
    }
  ]
}
```

## Simulate a saved rule set

```http
POST /api/simulations
Content-Type: application/json
```

```json
{
  "ruleSetId": 1,
  "page": 1,
  "pageSize": 50
}
```

## Simulate an unsaved draft

```json
{
  "draft": {
    "name": "Unsaved draft",
    "enabled": true,
    "rules": []
  },
  "page": 1,
  "pageSize": 50
}
```
