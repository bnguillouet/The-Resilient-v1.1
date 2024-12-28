using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[TestFixture]
public class SettlementTests
{
    private Settlement settlement;

    [SetUp]
    public void SetUp()
    {
        GameObject settlementObject = new GameObject();
        settlement = settlementObject.AddComponent<Settlement>();
        Settlement.Instance = settlement;
        Settlement.InitializeSettlement();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(settlement.gameObject);
    }

    [Test]
    public void Awake_SingleInstance()
    {
        // Arrange
        GameObject newSettlementObject = new GameObject();
        Settlement newSettlement = newSettlementObject.AddComponent<Settlement>();

        // Act
        newSettlement.Awake();

        // Assert
        Assert.AreEqual(settlement, Settlement.Instance);
        Object.DestroyImmediate(newSettlementObject);
    }

    [Test]
    public void ReinitializeSettlement_ClearsAndReinitializesLists()
    {
        // Arrange
        Settlement.CreateBuilding(new Vector2Int(0, 0));
        Settlement.CreateBorder(new Vector2Int(1, 1), true);

        // Act
        Settlement.ReinitializeSettlement();

        // Assert
        Assert.IsEmpty(Settlement.buildings);
        Assert.IsEmpty(Settlement.borders);
        Assert.IsEmpty(Settlement.blueprints);
        Assert.AreEqual(20, Settlement.freshHouseGarbage);
    }

    [Test]
    public void InitializeSettlement_InitializesListsAndSetsDefaults()
    {
        // Act
        Settlement.InitializeSettlement();

        // Assert
        Assert.IsNotNull(Settlement.buildings);
        Assert.IsNotNull(Settlement.borders);
        Assert.IsNotNull(Settlement.blueprints);
        Assert.AreEqual(20, Settlement.freshHouseGarbage);
    }

    [Test]
    public void CreateBuilding_AddsBuildingToList()
    {
        // Arrange
        BuildingBlueprint blueprint = ScriptableObject.CreateInstance<BuildingBlueprint>();
        Settlement.selectedBlueprint = blueprint;

        // Act
        Settlement.CreateBuilding(new Vector2Int(0, 0));

        // Assert
        Assert.AreEqual(1, Settlement.buildings.Count);
    }

    [Test]
    public void CreateBorder_AddsBorderToList()
    {
        // Arrange
        BuildingBlueprint blueprint = ScriptableObject.CreateInstance<BuildingBlueprint>();
        Settlement.selectedBlueprint = blueprint;

        // Act
        Settlement.CreateBorder(new Vector2Int(1, 1), true);

        // Assert
        Assert.AreEqual(1, Settlement.borders.Count);
    }

    [Test]
    public void CantConstructBorderFromAPoint_ReturnsCorrectBoolean()
    {
        // Arrange
        Vector2Int position = new Vector2Int(0, 0);

        // Act
        bool result = Settlement.CantConstructBorderFromAPoint(position);

        // Assert
        // Assuming HaveBorder method is implemented and returns 99 for this test case
        Assert.IsFalse(result);
    }
}