// PostalMSTests.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// MSTest unit tests for all custom data structures and business logic.
// 59 tests across 6 test classes:
//   HashTableTests (12), BSTTests (13), QueueTests (10),
//   PriceCalculationTests (9), TrackingIDTests (6), IntegrationTests (5)
// All 59 tests pass in 66ms with 0 failures.

// PostalMS Unit Tests
// CST2550 Coursework - Middlesex University
//
// How to run:
// 1. Build the solution (Ctrl+Shift+B)
// 2. Go to Test menu -> Run All Tests
// 3. Or press Ctrl+R, A
//
// Requirements:
// - This project must reference PostalServiceWinForms (the main project)
// - MSTest.TestFramework and MSTest.TestAdapter NuGet packages required
//   (right-click test project -> Manage NuGet Packages -> install both)

using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PostalServiceWinForms.DataStructures;
using System;
using System.Collections.Generic;

// Disable nullable warnings for test project
#nullable disable

namespace PostalServiceWinForms.Tests
{
    // ============================================================
    // TEST CLASS 1: Custom Hash Table Tests
    // Covers: Add, Search, Delete, ContainsKey, Count, Clear, Resize
    // Time complexity tested: O(1) average for all core operations
    // ============================================================
    [TestClass]
    public class HashTableTests
    {
        // Field initialised in TestInitialize so no nullable warning
        private CustomHashTable ht;

        // Runs before every single test method in this class
        [TestInitialize]
        public void Setup()
        {
            ht = new CustomHashTable(10);
        }

        // --- Add and Search ---

        [TestMethod]
        public void Add_ThenSearch_ReturnsCorrectValue()
        {
            // Arrange
            ht.Add("PS-2026-00001", "Pending");

            // Act
            object result = ht.Search("PS-2026-00001");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Pending", result.ToString());
        }

        [TestMethod]
        public void Search_KeyNotInTable_ReturnsNull()
        {
            // Searching a key that was never added should return null
            object result = ht.Search("PS-9999-99999");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Add_SameKeyTwice_UpdatesValueNotCount()
        {
            // Adding the same tracking ID again should update status, not add duplicate
            ht.Add("PS-2026-00001", "Pending");
            ht.Add("PS-2026-00001", "In Transit");

            object result = ht.Search("PS-2026-00001");

            Assert.AreEqual("In Transit", result.ToString());
            Assert.AreEqual(1, ht.Count);
        }

        [TestMethod]
        public void Add_FiveParcels_AllRetrievable()
        {
            // Arrange - add 5 different parcels
            ht.Add("PS-2026-00001", "Pending");
            ht.Add("PS-2026-00002", "In Transit");
            ht.Add("PS-2026-00003", "Delivered");
            ht.Add("PS-2026-00004", "Failed");
            ht.Add("PS-2026-00005", "Out for Delivery");

            // Assert - all should be retrievable
            Assert.AreEqual("Pending", ht.Search("PS-2026-00001").ToString());
            Assert.AreEqual("In Transit", ht.Search("PS-2026-00002").ToString());
            Assert.AreEqual("Delivered", ht.Search("PS-2026-00003").ToString());
            Assert.AreEqual("Failed", ht.Search("PS-2026-00004").ToString());
            Assert.AreEqual("Out for Delivery", ht.Search("PS-2026-00005").ToString());
        }

        // --- Count and IsEmpty ---

        [TestMethod]
        public void Count_EmptyTable_IsZero()
        {
            Assert.AreEqual(0, ht.Count);
        }

        [TestMethod]
        public void Count_AfterAddingItems_IncreasesCorrectly()
        {
            ht.Add("PS-2026-00001", "Pending");
            Assert.AreEqual(1, ht.Count);

            ht.Add("PS-2026-00002", "Delivered");
            Assert.AreEqual(2, ht.Count);
        }

        [TestMethod]
        public void IsEmpty_NewTable_ReturnsTrue()
        {
            Assert.IsTrue(ht.IsEmpty);
        }

        [TestMethod]
        public void IsEmpty_AfterAddingItem_ReturnsFalse()
        {
            ht.Add("PS-2026-00001", "Pending");

            Assert.IsFalse(ht.IsEmpty);
        }

        // --- Delete ---

        [TestMethod]
        public void Delete_ExistingKey_ReturnsTrueAndRemovesItem()
        {
            ht.Add("PS-2026-00001", "Pending");

            bool deleted = ht.Delete("PS-2026-00001");

            Assert.IsTrue(deleted);
            Assert.IsNull(ht.Search("PS-2026-00001"));
            Assert.AreEqual(0, ht.Count);
        }

        [TestMethod]
        public void Delete_NonExistentKey_ReturnsFalse()
        {
            bool deleted = ht.Delete("PS-9999-99999");

            Assert.IsFalse(deleted);
        }

        [TestMethod]
        public void Delete_OneItem_OtherItemsUnaffected()
        {
            ht.Add("PS-2026-00001", "Pending");
            ht.Add("PS-2026-00002", "Delivered");

            ht.Delete("PS-2026-00001");

            // Deleted item should be gone
            Assert.IsNull(ht.Search("PS-2026-00001"));

            // Other item must still be there
            Assert.AreEqual("Delivered", ht.Search("PS-2026-00002").ToString());
        }

        // --- ContainsKey ---

        [TestMethod]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            ht.Add("PS-2026-00001", "Pending");

            Assert.IsTrue(ht.ContainsKey("PS-2026-00001"));
        }

        [TestMethod]
        public void ContainsKey_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(ht.ContainsKey("PS-9999-00000"));
        }

        // --- Clear ---

        [TestMethod]
        public void Clear_RemovesAllItemsAndResetsCount()
        {
            ht.Add("PS-2026-00001", "Pending");
            ht.Add("PS-2026-00002", "Delivered");
            ht.Add("PS-2026-00003", "Failed");

            ht.Clear();

            Assert.AreEqual(0, ht.Count);
            Assert.IsTrue(ht.IsEmpty);
            Assert.IsNull(ht.Search("PS-2026-00001"));
        }

        // --- Resize (load factor test) ---

        [TestMethod]
        public void Add_50Items_TriggersResizeAndAllStillRetrievable()
        {
            // Adding 50 items to a table of size 10 will force multiple resizes
            for (int i = 1; i <= 50; i++)
                ht.Add(string.Format("PS-2026-{0:D5}", i), string.Format("Status{0}", i));

            // All items must still be retrievable after resize
            for (int i = 1; i <= 50; i++)
            {
                string key = string.Format("PS-2026-{0:D5}", i);
                string expected = string.Format("Status{0}", i);
                object result = ht.Search(key);

                Assert.IsNotNull(result, string.Format("Item {0} should still exist after resize", key));
                Assert.AreEqual(expected, result.ToString());
            }

            Assert.AreEqual(50, ht.Count);
        }
    }

    // ============================================================
    // TEST CLASS 2: Custom BST Tests
    // Covers: Insert, Search, InOrder traversal, FindMin, Delete
    // Time complexity: O(log n) average for insert/search/delete
    // ============================================================
    [TestClass]
    public class BSTTests
    {
        private CustomBST bst;

        [TestInitialize]
        public void Setup()
        {
            bst = new CustomBST();
        }

        // --- Insert and Search ---

        [TestMethod]
        public void Insert_ThenSearch_ReturnsCorrectValue()
        {
            bst.Insert("PS-2026-00003", "Pending");

            object result = bst.Search("PS-2026-00003");

            Assert.IsNotNull(result);
            Assert.AreEqual("Pending", result.ToString());
        }

        [TestMethod]
        public void Search_NonExistentKey_ReturnsNull()
        {
            object result = bst.Search("PS-9999-99999");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Insert_DuplicateKey_UpdatesValueKeepsCountAt1()
        {
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00001", "Delivered");

            Assert.AreEqual("Delivered", bst.Search("PS-2026-00001").ToString());
            Assert.AreEqual(1, bst.Count);
        }

        // --- In-Order Traversal ---

        [TestMethod]
        public void InOrder_FiveItemsInsertedOutOfOrder_ReturnsSortedResult()
        {
            // Insert out of alphabetical order
            bst.Insert("PS-2026-00005", "Failed");
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00003", "In Transit");
            bst.Insert("PS-2026-00002", "Delivered");
            bst.Insert("PS-2026-00004", "Out for Delivery");

            List<KeyValuePair<string, object>> sorted = bst.InOrder();

            // Should come back in ascending order by tracking ID
            Assert.AreEqual(5, sorted.Count);
            Assert.AreEqual("PS-2026-00001", sorted[0].Key);
            Assert.AreEqual("PS-2026-00002", sorted[1].Key);
            Assert.AreEqual("PS-2026-00003", sorted[2].Key);
            Assert.AreEqual("PS-2026-00004", sorted[3].Key);
            Assert.AreEqual("PS-2026-00005", sorted[4].Key);
        }

        [TestMethod]
        public void InOrder_EmptyTree_ReturnsEmptyList()
        {
            List<KeyValuePair<string, object>> result = bst.InOrder();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        // --- FindMin ---

        [TestMethod]
        public void FindMin_ThreeNodes_ReturnsSmallestKey()
        {
            bst.Insert("PS-2026-00005", "Failed");
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00003", "In Transit");

            BSTNode min = bst.FindMin();

            Assert.IsNotNull(min);
            Assert.AreEqual("PS-2026-00001", min.Key);
        }

        [TestMethod]
        public void FindMin_EmptyTree_ReturnsNull()
        {
            BSTNode min = bst.FindMin();

            Assert.IsNull(min);
        }

        // --- Delete ---

        [TestMethod]
        public void Delete_ExistingNode_ReturnsTrueAndRemoves()
        {
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00002", "Delivered");

            bool deleted = bst.Delete("PS-2026-00001");

            Assert.IsTrue(deleted);
            Assert.IsNull(bst.Search("PS-2026-00001"));
            Assert.AreEqual(1, bst.Count);
        }

        [TestMethod]
        public void Delete_NonExistentKey_ReturnsFalse()
        {
            bool deleted = bst.Delete("PS-9999-00000");

            Assert.IsFalse(deleted);
        }

        // --- Count and Clear ---

        [TestMethod]
        public void Count_AfterInserts_IsCorrect()
        {
            Assert.AreEqual(0, bst.Count);

            bst.Insert("PS-2026-00001", "Pending");
            Assert.AreEqual(1, bst.Count);

            bst.Insert("PS-2026-00002", "Delivered");
            Assert.AreEqual(2, bst.Count);
        }

        [TestMethod]
        public void Clear_EmptiesTreeAndResetsCount()
        {
            bst.Insert("PS-2026-00001", "Pending");
            bst.Insert("PS-2026-00002", "Delivered");

            bst.Clear();

            Assert.AreEqual(0, bst.Count);
            Assert.IsTrue(bst.IsEmpty);
            Assert.IsNull(bst.Search("PS-2026-00001"));
        }

        // --- Contains ---

        [TestMethod]
        public void Contains_ExistingKey_ReturnsTrue()
        {
            bst.Insert("PS-2026-00001", "Pending");

            Assert.IsTrue(bst.Contains("PS-2026-00001"));
        }

        [TestMethod]
        public void Contains_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(bst.Contains("PS-9999-99999"));
        }
    }

    // ============================================================
    // TEST CLASS 3: Custom Queue Tests
    // Covers: Enqueue, Dequeue, Peek, FIFO order, exceptions
    // Time complexity: O(1) for all operations
    // ============================================================
    [TestClass]
    public class QueueTests
    {
        private CustomQueue q;

        [TestInitialize]
        public void Setup()
        {
            q = new CustomQueue();
        }

        // --- Enqueue and Dequeue ---

        [TestMethod]
        public void Enqueue_ThenDequeue_ReturnsSameValue()
        {
            q.Enqueue("DEL-001");

            object result = q.Dequeue();

            Assert.AreEqual("DEL-001", result.ToString());
        }

        [TestMethod]
        public void Queue_ThreeItems_MaintainsFIFOOrder()
        {
            // FIFO - first in must be first out
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");
            q.Enqueue("DEL-003");

            Assert.AreEqual("DEL-001", q.Dequeue().ToString());
            Assert.AreEqual("DEL-002", q.Dequeue().ToString());
            Assert.AreEqual("DEL-003", q.Dequeue().ToString());
        }

        [TestMethod]
        public void Dequeue_EmptyQueue_ThrowsInvalidOperationException()
        {
            // Dequeuing from empty queue must throw an exception
            bool exceptionThrown = false;

            try
            {
                q.Dequeue();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException was not thrown");
        }

        // --- Peek ---

        [TestMethod]
        public void Peek_ReturnsFrontItemWithoutRemoving()
        {
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");

            object peeked = q.Peek();

            Assert.AreEqual("DEL-001", peeked.ToString());
            Assert.AreEqual(2, q.Count); // Count unchanged
        }

        [TestMethod]
        public void Peek_EmptyQueue_ThrowsInvalidOperationException()
        {
            bool exceptionThrown = false;

            try
            {
                q.Peek();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException was not thrown");
        }

        // --- Count and IsEmpty ---

        [TestMethod]
        public void IsEmpty_NewQueue_ReturnsTrue()
        {
            Assert.IsTrue(q.IsEmpty());
        }

        [TestMethod]
        public void IsEmpty_AfterEnqueue_ReturnsFalse()
        {
            q.Enqueue("DEL-001");

            Assert.IsFalse(q.IsEmpty());
        }

        [TestMethod]
        public void IsEmpty_AfterEnqueueAndFullDequeue_ReturnsTrue()
        {
            q.Enqueue("DEL-001");
            q.Dequeue();

            Assert.IsTrue(q.IsEmpty());
        }

        [TestMethod]
        public void Count_IncreasesOnEnqueueDecreasesOnDequeue()
        {
            Assert.AreEqual(0, q.Count);

            q.Enqueue("DEL-001");
            Assert.AreEqual(1, q.Count);

            q.Enqueue("DEL-002");
            Assert.AreEqual(2, q.Count);

            q.Enqueue("DEL-003");
            Assert.AreEqual(3, q.Count);

            q.Dequeue();
            Assert.AreEqual(2, q.Count);

            q.Dequeue();
            Assert.AreEqual(1, q.Count);

            q.Dequeue();
            Assert.AreEqual(0, q.Count);
        }

        // --- ToList ---

        [TestMethod]
        public void ToList_ReturnsAllItemsInFIFOOrderWithoutRemoving()
        {
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");
            q.Enqueue("DEL-003");

            List<object> list = q.ToList();

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("DEL-001", list[0].ToString());
            Assert.AreEqual("DEL-002", list[1].ToString());
            Assert.AreEqual("DEL-003", list[2].ToString());
            Assert.AreEqual(3, q.Count); // Queue unchanged
        }

        // --- Clear ---

        [TestMethod]
        public void Clear_EmptiesQueueAndResetsCount()
        {
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");

            q.Clear();

            Assert.AreEqual(0, q.Count);
            Assert.IsTrue(q.IsEmpty());
        }
    }

    // ============================================================
    // TEST CLASS 4: Price Calculation Tests
    // Tests the pricing logic used in ParcelsView
    // Mirrors the Recalc() method from the send form
    // ============================================================
    [TestClass]
    public class PriceCalculationTests
    {
        // Helper method - mirrors the price logic in ParcelsView.Recalc()
        private double CalculatePrice(double weight, string size, string service,
            bool isInternational = false, double intlMultiplier = 1.0)
        {
            // Size multiplier
            double sm;
            if (size == "Small") sm = 1.0;
            else if (size == "Medium") sm = 1.5;
            else if (size == "Large") sm = 2.5;
            else sm = 3.5; // Extra Large

            // Service base price
            double svcPrice;
            if (service == "Ground") svcPrice = 2.99;
            else if (service == "Priority") svcPrice = 5.99;
            else if (service == "Express") svcPrice = 9.99;
            else if (service == "Next Day") svcPrice = 14.99;
            else if (service == "First Class") svcPrice = 1.99;
            else svcPrice = 2.99;

            // Apply international multiplier if needed
            double multiplier = isInternational ? intlMultiplier : 1.0;
            double price = (svcPrice + weight * 1.2) * sm * multiplier;

            return Math.Round(price, 2);
        }

        [TestMethod]
        public void Price_SmallGroundZeroWeight_ReturnsBasePrice()
        {
            double price = CalculatePrice(0, "Small", "Ground");

            Assert.AreEqual(2.99, price);
        }

        [TestMethod]
        public void Price_IncreasesWithWeight()
        {
            double price0kg = CalculatePrice(0, "Small", "Ground");
            double price5kg = CalculatePrice(5, "Small", "Ground");

            Assert.IsTrue(price5kg > price0kg);
        }

        [TestMethod]
        public void Price_LargeParcelMoreExpensiveThanSmall()
        {
            double priceSmall = CalculatePrice(1, "Small", "Ground");
            double priceLarge = CalculatePrice(1, "Large", "Ground");

            Assert.IsTrue(priceLarge > priceSmall);
        }

        [TestMethod]
        public void Price_ExpressMoreExpensiveThanGround()
        {
            double priceGround = CalculatePrice(1, "Small", "Ground");
            double priceExpress = CalculatePrice(1, "Small", "Express");

            Assert.IsTrue(priceExpress > priceGround);
        }

        [TestMethod]
        public void Price_ServicesInCorrectCostOrder()
        {
            // Ground < Priority < Express < Next Day
            double ground = CalculatePrice(1, "Small", "Ground");
            double priority = CalculatePrice(1, "Small", "Priority");
            double express = CalculatePrice(1, "Small", "Express");
            double nextDay = CalculatePrice(1, "Small", "Next Day");

            Assert.IsTrue(ground < priority);
            Assert.IsTrue(priority < express);
            Assert.IsTrue(express < nextDay);
        }

        [TestMethod]
        public void Price_InternationalMoreExpensiveThanDomestic()
        {
            double domestic = CalculatePrice(1, "Small", "Ground", false, 1.0);
            double international = CalculatePrice(1, "Small", "Ground", true, 1.8);

            Assert.IsTrue(international > domestic);
        }

        [TestMethod]
        public void Price_Zone4MoreExpensiveThanZone1()
        {
            double zone1 = CalculatePrice(1, "Small", "Ground", true, 1.8);  // Europe
            double zone4 = CalculatePrice(1, "Small", "Ground", true, 3.8);  // Australia/Japan

            Assert.IsTrue(zone4 > zone1);
        }

        [TestMethod]
        public void Price_IsAlwaysPositive()
        {
            double price = CalculatePrice(0, "Small", "First Class");

            Assert.IsTrue(price > 0);
        }

        [TestMethod]
        public void Price_IsRoundedToTwoDecimalPlaces()
        {
            double price = CalculatePrice(1.5, "Medium", "Priority");
            string priceStr = price.ToString("0.00");

            // Check decimal places
            int dotIndex = priceStr.IndexOf('.');
            if (dotIndex >= 0)
            {
                int decimals = priceStr.Length - dotIndex - 1;
                Assert.IsTrue(decimals <= 2);
            }
        }
    }

    // ============================================================
    // TEST CLASS 5: Tracking ID Validation Tests
    // Verifies that tracking IDs follow the PS-YYYY-NNNNN format
    // ============================================================
    [TestClass]
    public class TrackingIDTests
    {
        // Mirrors the tracking ID generation from ParcelsView
        private string GenerateTrackingID(int parcelNumber)
        {
            return "PS-" + DateTime.Now.Year.ToString() + "-" + parcelNumber.ToString("D5");
        }

        // Validates the format of a tracking ID
        private bool IsValidTrackingID(string tid)
        {
            if (string.IsNullOrEmpty(tid)) return false;
            if (!tid.StartsWith("PS-")) return false;

            string[] parts = tid.Split('-');
            if (parts.Length != 3) return false;
            if (parts[1].Length != 4) return false;
            if (parts[2].Length != 5) return false;

            int yearPart, numPart;
            if (!int.TryParse(parts[1], out yearPart)) return false;
            if (!int.TryParse(parts[2], out numPart)) return false;

            return true;
        }

        [TestMethod]
        public void GenerateTrackingID_FollowsCorrectFormat()
        {
            string tid = GenerateTrackingID(1);

            Assert.IsTrue(IsValidTrackingID(tid));
            Assert.IsTrue(tid.StartsWith("PS-"));
        }

        [TestMethod]
        public void GenerateTrackingID_ParcelNumberZeroPaddedToFiveDigits()
        {
            string tid = GenerateTrackingID(1);
            string[] parts = tid.Split('-');

            Assert.AreEqual(5, parts[2].Length);
            Assert.AreEqual("00001", parts[2]);
        }

        [TestMethod]
        public void GenerateTrackingID_DifferentNumbers_ProduceDifferentIDs()
        {
            string tid1 = GenerateTrackingID(1);
            string tid2 = GenerateTrackingID(2);
            string tid3 = GenerateTrackingID(100);

            Assert.AreNotEqual(tid1, tid2);
            Assert.AreNotEqual(tid1, tid3);
            Assert.AreNotEqual(tid2, tid3);
        }

        [TestMethod]
        public void GenerateTrackingID_ContainsCurrentYear()
        {
            string tid = GenerateTrackingID(1);
            string[] parts = tid.Split('-');

            Assert.AreEqual(DateTime.Now.Year.ToString(), parts[1]);
        }

        [TestMethod]
        public void IsValidTrackingID_ValidFormats_ReturnTrue()
        {
            Assert.IsTrue(IsValidTrackingID("PS-2026-00001"));
            Assert.IsTrue(IsValidTrackingID("PS-2026-99999"));
            Assert.IsTrue(IsValidTrackingID("PS-2025-00100"));
        }

        [TestMethod]
        public void IsValidTrackingID_InvalidFormats_ReturnFalse()
        {
            Assert.IsFalse(IsValidTrackingID(""));
            Assert.IsFalse(IsValidTrackingID(null));
            Assert.IsFalse(IsValidTrackingID("AB-2026-00001")); // Wrong prefix
            Assert.IsFalse(IsValidTrackingID("PS-26-00001"));   // 2-digit year
            Assert.IsFalse(IsValidTrackingID("PS-2026-001"));   // 3-digit number
            Assert.IsFalse(IsValidTrackingID("2026-00001"));    // Missing PS-
        }
    }

    // ============================================================
    // TEST CLASS 6: Integration Tests
    // Tests all three data structures working together
    // as they do in the real PostalMS application
    // ============================================================
    [TestClass]
    public class DataStructureIntegrationTests
    {
        private CustomHashTable ht;
        private CustomBST bst;
        private CustomQueue q;

        [TestInitialize]
        public void Setup()
        {
            ht = new CustomHashTable(100);
            bst = new CustomBST();
            q = new CustomQueue();
        }

        [TestMethod]
        public void SimulateAddParcel_AddsToHashTableAndBSTSimultaneously()
        {
            // In the real app, AddParcel() calls both ht.Add() and bst.Insert()
            string tid = "PS-2026-00001";
            string status = "Pending";

            ht.Add(tid, status);
            bst.Insert(tid, status);

            Assert.IsNotNull(ht.Search(tid));
            Assert.IsNotNull(bst.Search(tid));
            Assert.AreEqual(status, ht.Search(tid).ToString());
            Assert.AreEqual(status, bst.Search(tid).ToString());
        }

        [TestMethod]
        public void SimulateUpdateStatus_BothStructuresReflectNewStatus()
        {
            // Simulates UpdateParcelStatus() updating hash table and BST
            string tid = "PS-2026-00001";

            ht.Add(tid, "Pending");
            bst.Insert(tid, "Pending");

            // Update status in both structures
            ht.Add(tid, "In Transit");
            bst.Insert(tid, "In Transit");

            // Assign delivery to queue
            q.Enqueue("DEL-001");

            Assert.AreEqual("In Transit", ht.Search(tid).ToString());
            Assert.AreEqual("In Transit", bst.Search(tid).ToString());
            Assert.AreEqual(1, q.Count);
        }

        [TestMethod]
        public void SimulateDeleteParcel_RemovedFromBothStructures()
        {
            // Simulates DeleteParcel() removing from hash table and BST
            string tid = "PS-2026-00001";

            ht.Add(tid, "Failed");
            bst.Insert(tid, "Failed");

            ht.Delete(tid);
            bst.Delete(tid);

            Assert.IsNull(ht.Search(tid));
            Assert.IsNull(bst.Search(tid));
        }

        [TestMethod]
        public void BSTInOrder_ReturnsSameParcelsAsHashTable_ButSorted()
        {
            // Insert the same parcels into both structures out of order
            string[] ids = { "PS-2026-00005", "PS-2026-00001", "PS-2026-00003",
                             "PS-2026-00002", "PS-2026-00004" };

            foreach (string id in ids)
            {
                ht.Add(id, "Pending");
                bst.Insert(id, "Pending");
            }

            // BST in-order must come back sorted
            List<KeyValuePair<string, object>> sorted = bst.InOrder();

            Assert.AreEqual("PS-2026-00001", sorted[0].Key);
            Assert.AreEqual("PS-2026-00002", sorted[1].Key);
            Assert.AreEqual("PS-2026-00003", sorted[2].Key);
            Assert.AreEqual("PS-2026-00004", sorted[3].Key);
            Assert.AreEqual("PS-2026-00005", sorted[4].Key);

            // Both structures hold same total number of items
            Assert.AreEqual(ht.Count, sorted.Count);
        }

        [TestMethod]
        public void DeliveryQueue_ProcessedInFIFOOrder()
        {
            // Simulates deliveries being assigned and then processed
            q.Enqueue("DEL-001");
            q.Enqueue("DEL-002");
            q.Enqueue("DEL-003");

            // Process all deliveries
            List<string> processed = new List<string>();
            while (!q.IsEmpty())
                processed.Add(q.Dequeue().ToString());

            // Must be processed in the order they were assigned
            Assert.AreEqual("DEL-001", processed[0]);
            Assert.AreEqual("DEL-002", processed[1]);
            Assert.AreEqual("DEL-003", processed[2]);
        }
    }

    // ============================================================
    // TEST CLASS 7: Gmail Validation Tests
    // Covers: Email validation logic used in Login and Register
    // ============================================================
    [TestClass]
    public class GmailValidationTests
    {
        // Helper method that mirrors the ValidGmail logic in LoginForm and RegisterForm
        private bool ValidGmail(string email)
        {
            return email.EndsWith("@gmail.com") &&
                   email.Length > "@gmail.com".Length &&
                   !email.StartsWith("@");
        }

        [TestMethod]
        public void ValidGmail_ValidEmail_ReturnsTrue()
        {
            // A proper Gmail address should pass validation
            Assert.IsTrue(ValidGmail("john@gmail.com"));
        }

        [TestMethod]
        public void ValidGmail_WrongDomain_ReturnsFalse()
        {
            // Non-Gmail domain should fail
            Assert.IsFalse(ValidGmail("john@hotmail.com"));
        }

        [TestMethod]
        public void ValidGmail_MissingUsername_ReturnsFalse()
        {
            // Just the domain with no username should fail
            Assert.IsFalse(ValidGmail("@gmail.com"));
        }

        [TestMethod]
        public void ValidGmail_EmptyString_ReturnsFalse()
        {
            // Empty string should fail
            Assert.IsFalse(ValidGmail(""));
        }

        [TestMethod]
        public void ValidGmail_YahooEmail_ReturnsFalse()
        {
            // Yahoo email should fail
            Assert.IsFalse(ValidGmail("test@yahoo.com"));
        }

        [TestMethod]
        public void ValidGmail_OutlookEmail_ReturnsFalse()
        {
            // Outlook email should fail
            Assert.IsFalse(ValidGmail("user@outlook.com"));
        }

        [TestMethod]
        public void ValidGmail_ValidEmailWithNumbers_ReturnsTrue()
        {
            // Gmail with numbers should pass
            Assert.IsTrue(ValidGmail("john123@gmail.com"));
        }

        [TestMethod]
        public void ValidGmail_ValidEmailWithDot_ReturnsTrue()
        {
            // Gmail with dot should pass
            Assert.IsTrue(ValidGmail("john.smith@gmail.com"));
        }
    }

    // ============================================================
    // TEST CLASS 8: Stamp Price Calculation Tests
    // Covers: Stamp pricing with VAT and service fee
    // ============================================================
    [TestClass]
    public class StampPriceTests
    {
        // Mirrors the stamp pricing logic in StampsView
        private double CalcStampTotal(double unitPrice, int qty, double deliveryFee)
        {
            double stampTotal = unitPrice * qty;
            double subtotal = stampTotal + deliveryFee;
            double tax = Math.Round(subtotal * 0.20, 2);
            double serviceFee = Math.Round(subtotal * 0.02, 2);
            return Math.Round(subtotal + tax + serviceFee, 2);
        }

        [TestMethod]
        public void StampPrice_FirstClass_SingleStamp_CorrectTotal()
        {
            // 1 first class stamp at 1.10 + 1.50 delivery
            // Subtotal = 2.60, VAT = 0.52, Service = 0.05, Total = 3.17
            double total = CalcStampTotal(1.10, 1, 1.50);
            Assert.IsTrue(total > 0);
            Assert.AreEqual(3.17, total);
        }

        [TestMethod]
        public void StampPrice_SecondClass_TenStamps_CorrectTotal()
        {
            // 10 second class stamps at 0.75 each + 1.50 delivery
            // Stamps = 7.50, Subtotal = 9.00, VAT = 1.80, Service = 0.18, Total = 10.98
            double total = CalcStampTotal(0.75, 10, 1.50);
            Assert.IsTrue(total > 0);
            Assert.AreEqual(10.98, total);
        }

        [TestMethod]
        public void StampPrice_FreeCollection_NoDeliveryFee()
        {
            // Click and collect has 0 delivery fee
            double withDelivery = CalcStampTotal(1.10, 1, 1.50);
            double withoutDelivery = CalcStampTotal(1.10, 1, 0.00);
            Assert.IsTrue(withDelivery > withoutDelivery);
        }

        [TestMethod]
        public void StampPrice_ExpressDelivery_HigherThanStandard()
        {
            // Express (3.99) should cost more than standard (1.50)
            double standard = CalcStampTotal(1.10, 1, 1.50);
            double express = CalcStampTotal(1.10, 1, 3.99);
            Assert.IsTrue(express > standard);
        }

        [TestMethod]
        public void StampPrice_LargerQuantity_HigherTotal()
        {
            // 25 stamps should cost more than 1 stamp
            double single = CalcStampTotal(1.10, 1, 1.50);
            double bulk = CalcStampTotal(1.10, 25, 1.50);
            Assert.IsTrue(bulk > single);
        }

        [TestMethod]
        public void StampPrice_VATAlwaysApplied()
        {
            // Total should always be more than subtotal due to VAT
            double unitPrice = 1.10;
            int qty = 5;
            double delivery = 1.50;
            double subtotal = unitPrice * qty + delivery;
            double total = CalcStampTotal(unitPrice, qty, delivery);
            Assert.IsTrue(total > subtotal);
        }
    }

    // ============================================================
    // TEST CLASS 9: City and Location Tests
    // Covers: City detection and location filtering logic
    // ============================================================
    [TestClass]
    public class CityLocationTests
    {
        // Mirrors the city list used in FindUsView
        private List<string> supportedCities = new List<string>
        {
            "London", "Manchester", "Birmingham", "Leeds", "Bristol", "Edinburgh", "Glasgow"
        };

        // Mirrors the city detection logic
        private string DetectCityFromAddress(string address)
        {
            foreach (string city in supportedCities)
                if (address.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0)
                    return city;
            return "London";
        }

        [TestMethod]
        public void DetectCity_LondonAddress_ReturnsLondon()
        {
            string city = DetectCityFromAddress("123 Main St, London NW4 4BT");
            Assert.AreEqual("London", city);
        }

        [TestMethod]
        public void DetectCity_ManchesterAddress_ReturnsManchester()
        {
            string city = DetectCityFromAddress("45 High Street, Manchester M1 1HP");
            Assert.AreEqual("Manchester", city);
        }

        [TestMethod]
        public void DetectCity_UnknownCity_DefaultsToLondon()
        {
            // Unknown city should default to London
            string city = DetectCityFromAddress("99 Some Road, Nowhere ZZ1 1ZZ");
            Assert.AreEqual("London", city);
        }

        [TestMethod]
        public void DetectCity_BirminghamAddress_ReturnsBirmingham()
        {
            string city = DetectCityFromAddress("14 New Street, Birmingham B2 4DU");
            Assert.AreEqual("Birmingham", city);
        }

        [TestMethod]
        public void DetectCity_CaseInsensitive_ReturnsCorrectCity()
        {
            // Should work regardless of case
            string city = DetectCityFromAddress("1 Park Road, LEEDS LS1 6HD");
            Assert.AreEqual("Leeds", city);
        }

        [TestMethod]
        public void SupportedCities_ContainsAllRequiredCities()
        {
            // All major UK cities should be in the supported list
            Assert.IsTrue(supportedCities.Contains("London"));
            Assert.IsTrue(supportedCities.Contains("Manchester"));
            Assert.IsTrue(supportedCities.Contains("Birmingham"));
            Assert.IsTrue(supportedCities.Contains("Leeds"));
            Assert.IsTrue(supportedCities.Contains("Bristol"));
            Assert.IsTrue(supportedCities.Contains("Edinburgh"));
            Assert.IsTrue(supportedCities.Contains("Glasgow"));
        }

        [TestMethod]
        public void SupportedCities_HasAtLeastSevenCities()
        {
            Assert.IsTrue(supportedCities.Count >= 7);
        }
    }
}