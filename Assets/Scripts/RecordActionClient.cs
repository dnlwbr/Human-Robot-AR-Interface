using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Actionlib;
using RosSharp.RosBridgeClient.MessageTypes.Actionlib;
using RosSharp.RosBridgeClient.MessageTypes.HriRobotArm;


namespace HumanRobotInterface
{
    public class RecordActionClient : ActionClient<RecordAction, RecordActionGoal, RecordActionResult, RecordActionFeedback, RecordGoal, RecordResult, RecordFeedback>
    {
        public RecordActionClient(string actionName, RosSocket rosSocket)
        {
            this.rosSocket = rosSocket;
            this.actionName = actionName;
            action = new RecordAction();
        }
 
        protected override RecordActionGoal GetActionGoal()
        {
            return action.action_goal;
        }

        protected override void OnFeedbackReceived()
        {
            // Not implemented for this particular application
        }

        protected override void OnResultReceived()
        {
            // Not implemented for this particular application
        }

        protected override void OnStatusUpdated()
        {
            // Not implemented for this particular application
        }
    }
}