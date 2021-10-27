/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



using RosSharp.RosBridgeClient.MessageTypes.Std;
using RosSharp.RosBridgeClient.MessageTypes.Vision;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;

namespace RosSharp.RosBridgeClient.MessageTypes.HriRobotArm
{
    public class RecordGoal : Message
    {
        public override string RosMessageName => "hri_robot_arm/RecordGoal";

        // goal
        public Header header { get; set; }
        public string class_name { get; set; }
        public BoundingBox3D bbox { get; set; }
        public Point gaze_point { get; set; }

        public RecordGoal()
        {
            this.header = new Header();
            this.class_name = "";
            this.bbox = new BoundingBox3D();
            this.gaze_point = new Point();
        }

        public RecordGoal(Header header, string class_name, BoundingBox3D bbox, Point gaze_point)
        {
            this.header = header;
            this.class_name = class_name;
            this.bbox = bbox;
            this.gaze_point = gaze_point;
        }
    }
}
