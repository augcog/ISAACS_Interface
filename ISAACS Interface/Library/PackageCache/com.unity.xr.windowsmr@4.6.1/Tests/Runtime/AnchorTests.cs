#if JENKINS

using NUnit.Framework;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.TestTools;


namespace UnityEngine.XR.WindowsMR.Tests
{
    public class AnchorTests : TestBaseSetup
    {
        static TrackableId defaultId = default(TrackableId);
        static Pose defaultPose = new Pose{ position = Vector3.forward, rotation = Quaternion.identity };

        [SetUp]
        public void SetUp()
        {
            ClearAnchorStore();
        }

        [TearDown]
        public void TearDown()
        {
            ClearAnchorStore();
        }

        bool CheckThereAreNoAnchors()
        {
            XRAnchorSubsystem rpsub = ActiveLoader.GetLoadedSubsystem<XRAnchorSubsystem>();
            Assert.NotNull(rpsub);

            TrackableChanges<XRAnchor>? currentRps = rpsub.GetChanges(Allocator.Temp);

            bool hasNoAnchors = (currentRps == null) ? false :
                currentRps?.added.Length == 0 &&
                currentRps?.removed.Length == 0 &&
                currentRps?.updated.Length == 0;

            return hasNoAnchors;
        }

        IEnumerator AddAndCheckAnchor(Action<TrackableId> callback)
        {
            XRAnchorSubsystem rpsub = ActiveLoader.GetLoadedSubsystem<XRAnchorSubsystem>();
            Assert.NotNull(rpsub);

            XRAnchor rp;
            bool ret = rpsub.TryAddAnchor(defaultPose, out rp);
            Assert.IsTrue(ret);
            Assert.AreNotEqual(defaultId, rp.trackableId);
            Assert.AreEqual(defaultPose, rp.pose);

            yield return null;

            TrackableChanges<XRAnchor>? currentRps = rpsub.GetChanges(Allocator.Temp);
            Assert.IsNotNull(currentRps);
            Assert.AreNotEqual(0, currentRps?.added.Length ?? 0);

            if (callback != null)
                callback.Invoke(currentRps?.added[0].trackableId ?? defaultId);
        }

        IEnumerator RemoveAndCheckAnchor( TrackableId id, Action<TrackableId> callback)
        {
            XRAnchorSubsystem rpsub = ActiveLoader.GetLoadedSubsystem<XRAnchorSubsystem>();
            Assert.NotNull(rpsub);

            rpsub.TryRemoveAnchor(id);

            yield return null;

            TrackableChanges<XRAnchor>? currentRps = rpsub.GetChanges(Allocator.Temp);
            Assert.IsNotNull(currentRps);
            Assert.AreNotEqual(0, currentRps?.removed.Length ?? 0);

            TrackableId? rpRemoved = currentRps?.removed[0] ?? null;
            if (callback != null)
                callback.Invoke(rpRemoved ?? defaultId);

        }

        XRAnchorStore GetAnchorStore()
        {
            XRAnchorSubsystem rpsub = ActiveLoader.GetLoadedSubsystem<XRAnchorSubsystem>();
            Assert.NotNull(rpsub);

            var store = rpsub.TryGetAnchorStoreAsync().Result;

            Assert.NotNull(store);
            return store;
        }

        bool PersistReferencePoint(TrackableId rpId, string name)
        {
            using (var store = GetAnchorStore())
            {
                return store?.TryPersistAnchor(rpId, name) ?? false;
            }
        }

        bool CheckPersistedAnchorsContainAnchorWithName(string name)
        {
            using (var store = GetAnchorStore())
            {
                var anchorNames = store?.PersistedAnchorNames ?? new List<string>();
                foreach (var anchorName in anchorNames)
                {
                    if (String.Compare(name, anchorName) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void ClearAnchorStore()
        {
            using (var store = GetAnchorStore())
            {
                store?.Clear();
            }
        }

        void LoadAnchorFromStore(string name, Action<TrackableId> callback)
        {
            using (var store = GetAnchorStore())
            {
                var tid = store.LoadAnchor(name);

                callback.Invoke(tid);
            }
        }

        enum UpdateState
        {
            Added,
            Updated,
            Removed,
            None
        }

        bool CheckAnchorUpdateState(TrackableId id, UpdateState updateState)
        {
            XRAnchorSubsystem rpsub = ActiveLoader.GetLoadedSubsystem<XRAnchorSubsystem>();
            Assert.NotNull(rpsub);

            TrackableChanges<XRAnchor>? currentRps = rpsub.GetChanges(Allocator.Temp);
            Assert.IsNotNull(currentRps);

            XRAnchor? rp = null;
            TrackableId idToCheck = defaultId;

            switch (updateState)
            {
                case UpdateState.Added:
                    Assert.AreNotEqual(0, currentRps?.added.Length ?? 0);
                    rp = currentRps?.added[0] ?? null;
                    idToCheck = rp?.trackableId ?? defaultId;
                    break;
                case UpdateState.Updated:
                    Assert.AreNotEqual(0, currentRps?.updated.Length ?? 0);
                    rp = currentRps?.updated[0] ?? null;
                    idToCheck = rp?.trackableId ?? defaultId;
                    break;

                case UpdateState.Removed:
                    Assert.AreNotEqual(0, currentRps?.removed.Length ?? 0);
                    idToCheck = currentRps?.removed[0] ?? defaultId;
                    break;
                case UpdateState.None:
                default:
                    return false;
            }


            return idToCheck == id;
        }

        [UnityTest]
        public IEnumerator AddingAnchorReturnsThatAnchor()
        {
            yield return AddAndCheckAnchor((tid) => {
                Assert.AreNotEqual(defaultId, tid);
            });
        }

        [UnityTest]
        public IEnumerator RemovingAnchorReturnsMarkedAnchor()
        {
            TrackableId rpId = defaultId;
            yield return AddAndCheckAnchor((tid) => {
                Assert.AreNotEqual(rpId, tid);
                rpId = tid;
            });

            Assert.AreNotEqual(defaultId, rpId);

            yield return RemoveAndCheckAnchor(rpId, (tid) => {
                Assert.AreEqual(rpId, tid);
            });

        }

        [UnityTest]
        public IEnumerator PersistAnchorToStore()
        {
            const string testRpName = "unity://anchors/Persist Test Anchor";

            ClearAnchorStore();

            TrackableId rpId = defaultId;
            yield return AddAndCheckAnchor((tid) => {
                Assert.AreNotEqual(rpId, tid);
                rpId = tid;
            });

            Assert.AreNotEqual(defaultId, rpId);

            bool ret = PersistReferencePoint(rpId, testRpName);
            Assert.IsTrue(ret);

            ret =  CheckPersistedAnchorsContainAnchorWithName(testRpName);
            Assert.IsTrue(ret);

            ClearAnchorStore();

            ret =  CheckPersistedAnchorsContainAnchorWithName(testRpName);
            Assert.IsFalse(ret);

            yield return RemoveAndCheckAnchor(rpId, (tid) => {
                Assert.AreEqual(rpId, tid);
            });

            Assert.IsTrue(CheckThereAreNoAnchors());
        }


        [UnityTest]
        public IEnumerator LoadFromAnchorToStore()
        {
            ClearAnchorStore();

            const string testRpName = "unity://anchors/Persist Test Anchor";

            TrackableId rpId = defaultId;
            yield return AddAndCheckAnchor((tid) => {
                Assert.AreNotEqual(rpId, tid);
                rpId = tid;
            });

            Assert.AreNotEqual(defaultId, rpId);

            bool ret = PersistReferencePoint(rpId, testRpName);
            Assert.IsTrue(ret);


            ret =  CheckPersistedAnchorsContainAnchorWithName(testRpName);
            Assert.IsTrue(ret);

            yield return RemoveAndCheckAnchor(rpId, (tid) => {
                Assert.AreEqual(rpId, tid);
            });

            TrackableId newId = defaultId;
            LoadAnchorFromStore(testRpName, (tid) => {
                newId = tid;
            });

            Assert.AreNotEqual(defaultId, newId);

            yield return null;

            bool wasAdded = CheckAnchorUpdateState(newId, UpdateState.Added);

            Assert.True(wasAdded);

            ClearAnchorStore();

        }

    }
}

#endif //JENKINS
