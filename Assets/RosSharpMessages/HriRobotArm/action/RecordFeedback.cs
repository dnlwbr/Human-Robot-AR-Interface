/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



namespace RosSharp.RosBridgeClient.MessageTypes.HriRobotArm
{
    public class RecordFeedback : Message
    {
        public override string RosMessageName => "hri_robot_arm/RecordFeedback";

        // feedback
        public double progress { get; set; }

        public RecordFeedback()
        {
            this.progress = 0.0;
        }

        public RecordFeedback(double progress)
        {
            this.progress = progress;
        }
    }
}
