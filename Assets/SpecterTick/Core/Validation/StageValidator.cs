using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core.Validation
{
    public static class StageValidator
    {
        private static readonly List<GridPosition> EmptyWalls = new List<GridPosition>();
        private static readonly List<SwitchPlacement> EmptySwitches = new List<SwitchPlacement>();
        private static readonly List<DoorPlacement> EmptyDoors = new List<DoorPlacement>();

        public static StageValidationResult Validate(StageDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var errors = new List<StageValidationError>();
            var seen = new HashSet<StageValidationError>();

            ValidateNullCollections(definition, errors, seen);
            ValidateDimensions(definition, errors, seen);
            ValidateUnsetPositions(definition, errors, seen);
            ValidateSwitchIds(definition, errors, seen);
            ValidateDoorIds(definition, errors, seen);
            ValidateDuplicateSwitchIds(definition, errors, seen);
            ValidateDuplicateDoorIds(definition, errors, seen);
            ValidateDoorReferences(definition, errors, seen);

            var dimensionsValid = definition.Width > 0 && definition.Height > 0;
            if (dimensionsValid)
            {
                ValidateOutOfRange(definition, errors, seen);
            }

            ValidatePlacement(definition, errors, seen);

            if (errors.Count > 0)
            {
                return StageValidationResult.Failure(errors);
            }

            return StageValidationResult.Success(BuildValidatedStage(definition));
        }

        private static void ValidateNullCollections(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            if (definition.Walls == null)
            {
                AddError(errors, seen, StageValidationErrorCode.WallsCollectionNull,
                    new StageValidationSubject(StageElementKind.Wall),
                    "Walls collection is null.");
            }

            if (definition.Switches == null)
            {
                AddError(errors, seen, StageValidationErrorCode.SwitchesCollectionNull,
                    new StageValidationSubject(StageElementKind.Switch),
                    "Switches collection is null.");
            }

            if (definition.Doors == null)
            {
                AddError(errors, seen, StageValidationErrorCode.DoorsCollectionNull,
                    new StageValidationSubject(StageElementKind.Door),
                    "Doors collection is null.");
            }
        }

        private static void ValidateDimensions(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            if (definition.Width <= 0)
            {
                AddError(errors, seen, StageValidationErrorCode.WidthInvalid,
                    new StageValidationSubject(StageElementKind.Stage),
                    "Stage width must be greater than zero.");
            }

            if (definition.Height <= 0)
            {
                AddError(errors, seen, StageValidationErrorCode.HeightInvalid,
                    new StageValidationSubject(StageElementKind.Stage),
                    "Stage height must be greater than zero.");
            }
        }

        private static void ValidateUnsetPositions(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            if (!definition.PlayerPosition.HasValue)
            {
                AddError(errors, seen, StageValidationErrorCode.PlayerPositionNotSet,
                    new StageValidationSubject(StageElementKind.Player),
                    "Player position is not set.");
            }

            if (!definition.SpecterPosition.HasValue)
            {
                AddError(errors, seen, StageValidationErrorCode.SpecterPositionNotSet,
                    new StageValidationSubject(StageElementKind.Specter),
                    "Specter position is not set.");
            }

            if (!definition.ExitPosition.HasValue)
            {
                AddError(errors, seen, StageValidationErrorCode.ExitPositionNotSet,
                    new StageValidationSubject(StageElementKind.Exit),
                    "Exit position is not set.");
            }
        }

        private static void ValidateSwitchIds(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            var switches = definition.Switches ?? EmptySwitches;
            for (var i = 0; i < switches.Count; i++)
            {
                if (!switches[i].Id.IsValid)
                {
                    AddError(errors, seen, StageValidationErrorCode.InvalidSwitchId,
                        new StageValidationSubject(StageElementKind.Switch, i, switches[i].Position, switches[i].Id),
                        "Switch ID is invalid.");
                }
            }
        }

        private static void ValidateDoorIds(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            var doors = definition.Doors ?? EmptyDoors;
            for (var i = 0; i < doors.Count; i++)
            {
                if (!doors[i].Id.IsValid)
                {
                    AddError(errors, seen, StageValidationErrorCode.InvalidDoorId,
                        new StageValidationSubject(StageElementKind.Door, i, doors[i].Position, default, doors[i].Id),
                        "Door ID is invalid.");
                }
            }
        }

        private static void ValidateDuplicateSwitchIds(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            var switches = definition.Switches ?? EmptySwitches;
            var counts = new Dictionary<SwitchId, int>();
            for (var i = 0; i < switches.Count; i++)
            {
                var id = switches[i].Id;
                if (!id.IsValid)
                {
                    continue;
                }

                if (!counts.ContainsKey(id))
                {
                    counts[id] = 0;
                }

                counts[id]++;
            }

            var duplicateIds = new List<SwitchId>();
            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    duplicateIds.Add(pair.Key);
                }
            }

            duplicateIds.Sort((a, b) => string.Compare(a.RawValue, b.RawValue, StringComparison.Ordinal));
            for (var i = 0; i < duplicateIds.Count; i++)
            {
                var id = duplicateIds[i];
                AddError(errors, seen, StageValidationErrorCode.DuplicateSwitchId,
                    new StageValidationSubject(StageElementKind.Switch, -1, null, id),
                    "Duplicate switch ID.");
            }
        }

        private static void ValidateDuplicateDoorIds(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            var doors = definition.Doors ?? EmptyDoors;
            var counts = new Dictionary<DoorId, int>();
            for (var i = 0; i < doors.Count; i++)
            {
                var id = doors[i].Id;
                if (!id.IsValid)
                {
                    continue;
                }

                if (!counts.ContainsKey(id))
                {
                    counts[id] = 0;
                }

                counts[id]++;
            }

            var duplicateIds = new List<DoorId>();
            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    duplicateIds.Add(pair.Key);
                }
            }

            duplicateIds.Sort((a, b) => string.Compare(a.RawValue, b.RawValue, StringComparison.Ordinal));
            for (var i = 0; i < duplicateIds.Count; i++)
            {
                var id = duplicateIds[i];
                AddError(errors, seen, StageValidationErrorCode.DuplicateDoorId,
                    new StageValidationSubject(StageElementKind.Door, -1, null, default, id),
                    "Duplicate door ID.");
            }
        }

        private static void ValidateDoorReferences(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            var doors = definition.Doors ?? EmptyDoors;
            var definedSwitches = BuildValidSwitchIdSet(definition);

            for (var doorIndex = 0; doorIndex < doors.Count; doorIndex++)
            {
                var door = doors[doorIndex];
                var references = door.SwitchReferences;
                if (references == null || references.Count == 0)
                {
                    AddError(errors, seen, StageValidationErrorCode.DoorSwitchReferencesEmpty,
                        new StageValidationSubject(StageElementKind.Door, doorIndex, door.Position, default, door.Id),
                        "Door switch references are empty.");
                    continue;
                }

                var seenInDoor = new HashSet<SwitchId>();
                for (var refIndex = 0; refIndex < references.Count; refIndex++)
                {
                    var switchId = references[refIndex];
                    if (!switchId.IsValid)
                    {
                        AddError(errors, seen, StageValidationErrorCode.InvalidDoorSwitchReference,
                            new StageValidationSubject(
                                StageElementKind.Door,
                                doorIndex,
                                door.Position,
                                switchId,
                                door.Id,
                                default,
                                refIndex),
                            "Door switch reference ID is invalid.");
                        continue;
                    }

                    if (!seenInDoor.Add(switchId))
                    {
                        AddError(errors, seen, StageValidationErrorCode.DuplicateDoorSwitchReference,
                            new StageValidationSubject(
                                StageElementKind.Door,
                                doorIndex,
                                door.Position,
                                switchId,
                                door.Id),
                            "Duplicate switch reference within door.");
                    }

                    if (!definedSwitches.Contains(switchId))
                    {
                        AddError(errors, seen, StageValidationErrorCode.DoorSwitchReferenceNotFound,
                            new StageValidationSubject(
                                StageElementKind.Door,
                                doorIndex,
                                door.Position,
                                switchId,
                                door.Id),
                            "Referenced switch ID was not found.");
                    }
                }
            }
        }

        private static HashSet<SwitchId> BuildValidSwitchIdSet(StageDefinition definition)
        {
            var set = new HashSet<SwitchId>();
            var switches = definition.Switches ?? EmptySwitches;
            for (var i = 0; i < switches.Count; i++)
            {
                var id = switches[i].Id;
                if (id.IsValid)
                {
                    set.Add(id);
                }
            }

            return set;
        }

        private static void ValidateOutOfRange(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            var width = definition.Width;
            var height = definition.Height;

            if (definition.PlayerPosition.HasValue)
            {
                var position = definition.PlayerPosition.Value;
                if (!IsInRange(position, width, height))
                {
                    AddError(errors, seen, StageValidationErrorCode.PlayerPositionOutOfRange,
                        new StageValidationSubject(StageElementKind.Player, -1, position),
                        "Player position is out of range.");
                }
            }

            if (definition.SpecterPosition.HasValue)
            {
                var position = definition.SpecterPosition.Value;
                if (!IsInRange(position, width, height))
                {
                    AddError(errors, seen, StageValidationErrorCode.SpecterPositionOutOfRange,
                        new StageValidationSubject(StageElementKind.Specter, -1, position),
                        "Specter position is out of range.");
                }
            }

            if (definition.ExitPosition.HasValue)
            {
                var position = definition.ExitPosition.Value;
                if (!IsInRange(position, width, height))
                {
                    AddError(errors, seen, StageValidationErrorCode.ExitPositionOutOfRange,
                        new StageValidationSubject(StageElementKind.Exit, -1, position),
                        "Exit position is out of range.");
                }
            }

            var walls = definition.Walls ?? EmptyWalls;
            for (var i = 0; i < walls.Count; i++)
            {
                if (!IsInRange(walls[i], width, height))
                {
                    AddError(errors, seen, StageValidationErrorCode.WallPositionOutOfRange,
                        new StageValidationSubject(StageElementKind.Wall, i, walls[i]),
                        "Wall position is out of range.");
                }
            }

            var switches = definition.Switches ?? EmptySwitches;
            for (var i = 0; i < switches.Count; i++)
            {
                var placement = switches[i];
                if (!IsInRange(placement.Position, width, height))
                {
                    AddError(errors, seen, StageValidationErrorCode.SwitchPositionOutOfRange,
                        new StageValidationSubject(
                            StageElementKind.Switch,
                            i,
                            placement.Position,
                            placement.Id.IsValid ? placement.Id : default),
                        "Switch position is out of range.");
                }
            }

            var doors = definition.Doors ?? EmptyDoors;
            for (var i = 0; i < doors.Count; i++)
            {
                var placement = doors[i];
                if (!IsInRange(placement.Position, width, height))
                {
                    AddError(errors, seen, StageValidationErrorCode.DoorPositionOutOfRange,
                        new StageValidationSubject(
                            StageElementKind.Door,
                            i,
                            placement.Position,
                            default,
                            placement.Id.IsValid ? placement.Id : default),
                        "Door position is out of range.");
                }
            }
        }

        private static void ValidatePlacement(
            StageDefinition definition,
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen)
        {
            var byPosition = new Dictionary<GridPosition, List<StageElementKind>>();

            if (definition.PlayerPosition.HasValue)
            {
                AddKindAt(byPosition, definition.PlayerPosition.Value, StageElementKind.Player);
            }

            if (definition.SpecterPosition.HasValue)
            {
                AddKindAt(byPosition, definition.SpecterPosition.Value, StageElementKind.Specter);
            }

            if (definition.ExitPosition.HasValue)
            {
                AddKindAt(byPosition, definition.ExitPosition.Value, StageElementKind.Exit);
            }

            var walls = definition.Walls ?? EmptyWalls;
            for (var i = 0; i < walls.Count; i++)
            {
                AddKindAt(byPosition, walls[i], StageElementKind.Wall);
            }

            var switches = definition.Switches ?? EmptySwitches;
            for (var i = 0; i < switches.Count; i++)
            {
                AddKindAt(byPosition, switches[i].Position, StageElementKind.Switch);
            }

            var doors = definition.Doors ?? EmptyDoors;
            for (var i = 0; i < doors.Count; i++)
            {
                AddKindAt(byPosition, doors[i].Position, StageElementKind.Door);
            }

            var sameKindErrors = new List<StageValidationError>();
            var crossKindErrors = new List<StageValidationError>();

            var positions = new List<GridPosition>(byPosition.Keys);
            positions.Sort(ComparePosition);

            for (var p = 0; p < positions.Count; p++)
            {
                var position = positions[p];
                var kinds = byPosition[position];
                var counts = CountKinds(kinds);

                for (var k = 0; k < PlacementConflictPairs.SameKindOverlapKinds.Count; k++)
                {
                    var kind = PlacementConflictPairs.SameKindOverlapKinds[k];
                    if (counts.TryGetValue(kind, out var count) && count >= 2)
                    {
                        sameKindErrors.Add(new StageValidationError(
                            StageValidationErrorCode.SameKindPositionOverlap,
                            new StageValidationSubject(kind, -1, position),
                            "Same-kind elements overlap at the same position."));
                    }
                }

                for (var pairIndex = 0; pairIndex < PlacementConflictPairs.CrossKindPairsInOrder.Count; pairIndex++)
                {
                    var pair = PlacementConflictPairs.CrossKindPairsInOrder[pairIndex];
                    if (HasKind(counts, pair.KindA) && HasKind(counts, pair.KindB))
                    {
                        PlacementConflictPairs.NormalizeKinds(pair.KindA, pair.KindB, out var kindA, out var kindB);
                        crossKindErrors.Add(new StageValidationError(
                            StageValidationErrorCode.CrossKindPositionConflict,
                            new StageValidationSubject(kindA, -1, position, default, default, kindB),
                            "Cross-kind placement conflict."));
                    }
                }
            }

            sameKindErrors.Sort(CompareSameKindError);
            crossKindErrors.Sort(CompareCrossKindError);

            for (var i = 0; i < sameKindErrors.Count; i++)
            {
                AddError(errors, seen, sameKindErrors[i]);
            }

            for (var i = 0; i < crossKindErrors.Count; i++)
            {
                AddError(errors, seen, crossKindErrors[i]);
            }
        }

        private static ValidatedStage BuildValidatedStage(StageDefinition definition)
        {
            var walls = definition.Walls ?? EmptyWalls;
            var wallCopy = new GridPosition[walls.Count];
            for (var i = 0; i < walls.Count; i++)
            {
                wallCopy[i] = walls[i];
            }

            var switches = definition.Switches ?? EmptySwitches;
            var switchCopy = new StageSwitch[switches.Count];
            for (var i = 0; i < switches.Count; i++)
            {
                switchCopy[i] = new StageSwitch(switches[i].Id, switches[i].Position);
            }

            var doors = definition.Doors ?? EmptyDoors;
            var doorCopy = new StageDoor[doors.Count];
            for (var i = 0; i < doors.Count; i++)
            {
                var door = doors[i];
                var references = door.SwitchReferences ?? new List<SwitchId>();
                var referenceCopy = new SwitchId[references.Count];
                for (var j = 0; j < references.Count; j++)
                {
                    referenceCopy[j] = references[j];
                }

                doorCopy[i] = new StageDoor(door.Id, door.Position, referenceCopy);
            }

            return new ValidatedStage(
                definition.Width,
                definition.Height,
                definition.PlayerPosition.Value,
                definition.SpecterPosition.Value,
                definition.ExitPosition.Value,
                wallCopy,
                switchCopy,
                doorCopy);
        }

        private static bool IsInRange(GridPosition position, int width, int height)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < width && position.Y < height;
        }

        private static void AddKindAt(
            Dictionary<GridPosition, List<StageElementKind>> byPosition,
            GridPosition position,
            StageElementKind kind)
        {
            if (!byPosition.TryGetValue(position, out var kinds))
            {
                kinds = new List<StageElementKind>();
                byPosition[position] = kinds;
            }

            kinds.Add(kind);
        }

        private static Dictionary<StageElementKind, int> CountKinds(List<StageElementKind> kinds)
        {
            var counts = new Dictionary<StageElementKind, int>();
            for (var i = 0; i < kinds.Count; i++)
            {
                var kind = kinds[i];
                if (!counts.ContainsKey(kind))
                {
                    counts[kind] = 0;
                }

                counts[kind]++;
            }

            return counts;
        }

        private static bool HasKind(Dictionary<StageElementKind, int> counts, StageElementKind kind)
        {
            return counts.TryGetValue(kind, out var count) && count > 0;
        }

        private static int ComparePosition(GridPosition a, GridPosition b)
        {
            var compareX = a.X.CompareTo(b.X);
            return compareX != 0 ? compareX : a.Y.CompareTo(b.Y);
        }

        private static int CompareSameKindError(StageValidationError a, StageValidationError b)
        {
            var kindCompare = ((int)a.Subject.Kind).CompareTo((int)b.Subject.Kind);
            if (kindCompare != 0)
            {
                return kindCompare;
            }

            return ComparePosition(a.Subject.Position.Value, b.Subject.Position.Value);
        }

        private static int CompareCrossKindError(StageValidationError a, StageValidationError b)
        {
            var pairIndexA = GetCrossPairIndex(a.Subject.Kind, a.Subject.OtherKind);
            var pairIndexB = GetCrossPairIndex(b.Subject.Kind, b.Subject.OtherKind);
            var pairCompare = pairIndexA.CompareTo(pairIndexB);
            if (pairCompare != 0)
            {
                return pairCompare;
            }

            return ComparePosition(a.Subject.Position.Value, b.Subject.Position.Value);
        }

        private static int GetCrossPairIndex(StageElementKind kindA, StageElementKind kindB)
        {
            PlacementConflictPairs.NormalizeKinds(kindA, kindB, out var normalizedA, out var normalizedB);
            for (var i = 0; i < PlacementConflictPairs.CrossKindPairsInOrder.Count; i++)
            {
                var pair = PlacementConflictPairs.CrossKindPairsInOrder[i];
                if (pair.KindA == normalizedA && pair.KindB == normalizedB)
                {
                    return i;
                }
            }

            return int.MaxValue;
        }

        private static void AddError(
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen,
            StageValidationErrorCode code,
            StageValidationSubject subject,
            string message)
        {
            AddError(errors, seen, new StageValidationError(code, subject, message));
        }

        private static void AddError(
            List<StageValidationError> errors,
            HashSet<StageValidationError> seen,
            StageValidationError error)
        {
            if (seen.Add(error))
            {
                errors.Add(error);
            }
        }
    }
}
