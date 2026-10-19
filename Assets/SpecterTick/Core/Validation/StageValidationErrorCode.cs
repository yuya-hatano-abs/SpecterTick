namespace SpecterTick.Core.Validation
{
    public enum StageValidationErrorCode
    {
        WallsCollectionNull = 0,
        SwitchesCollectionNull = 1,
        DoorsCollectionNull = 2,
        WidthInvalid = 3,
        HeightInvalid = 4,
        PlayerPositionNotSet = 5,
        SpecterPositionNotSet = 6,
        ExitPositionNotSet = 7,
        InvalidSwitchId = 8,
        InvalidDoorId = 9,
        DuplicateSwitchId = 10,
        DuplicateDoorId = 11,
        DoorSwitchReferencesEmpty = 12,
        DuplicateDoorSwitchReference = 13,
        DoorSwitchReferenceNotFound = 14,
        InvalidDoorSwitchReference = 15,
        PlayerPositionOutOfRange = 16,
        SpecterPositionOutOfRange = 17,
        ExitPositionOutOfRange = 18,
        WallPositionOutOfRange = 19,
        SwitchPositionOutOfRange = 20,
        DoorPositionOutOfRange = 21,
        SameKindPositionOverlap = 22,
        CrossKindPositionConflict = 23
    }
}
