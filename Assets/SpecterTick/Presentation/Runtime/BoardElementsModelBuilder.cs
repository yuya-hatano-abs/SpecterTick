using System;
using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public static class BoardElementsModelBuilder
    {
        public static BoardElementsModel Build(GridPuzzleEngineSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var stage = session.Stage;
            var switches = stage.Switches;
            var doors = stage.Doors;
            var count = 1 + switches.Count + doors.Count;
            var elements = new BoardElement[count];
            var index = 0;

            elements[index++] = new BoardElement(
                BoardElementKind.Exit,
                stage.ExitPosition,
                session.IsExitOpen);

            for (var i = 0; i < switches.Count; i++)
            {
                var sw = switches[i];
                elements[index++] = new BoardElement(
                    BoardElementKind.Switch,
                    sw.Position,
                    session.State.IsSwitchActivated(sw.Id));
            }

            for (var i = 0; i < doors.Count; i++)
            {
                var door = doors[i];
                elements[index++] = new BoardElement(
                    BoardElementKind.Door,
                    door.Position,
                    session.IsDoorOpen(door.Id));
            }

            return new BoardElementsModel(elements);
        }
    }
}
