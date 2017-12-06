using NosSharp.Enums;
using OpenNos.GameObject.Map;

namespace OpenNos.GameObject.Event
{
    public class EventContainer
    {
        #region Instantiation

        public EventContainer(MapInstance mapInstance, EventActionType eventActionType, object param)
        {
            MapInstance = mapInstance;
            EventActionType = eventActionType;
            Parameter = param;
        }

        #endregion

        #region Properties

        public EventActionType EventActionType { get; private set; }

        public MapInstance MapInstance { get; set; }

        public object Parameter { get; set; }

        #endregion
    }
}