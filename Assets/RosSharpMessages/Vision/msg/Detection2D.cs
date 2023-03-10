/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



using RosSharp.RosBridgeClient.MessageTypes.Std;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;

namespace RosSharp.RosBridgeClient.MessageTypes.Vision
{
    public class Detection2D : Message
    {
        public override string RosMessageName => "vision_msgs/Detection2D";

        //  Defines a 2D detection result.
        // 
        //  This is similar to a 2D classification, but includes position information,
        //    allowing a classification result for a specific crop or image point to
        //    to be located in the larger image.
        public Header header { get; set; }
        //  Class probabilities
        public ObjectHypothesisWithPose[] results { get; set; }
        //  2D bounding box surrounding the object.
        public BoundingBox2D bbox { get; set; }
        //  The 2D data that generated these results (i.e. region proposal cropped out of
        //    the image). Not required for all use cases, so it may be empty.
        public Image source_img { get; set; }

        public Detection2D()
        {
            this.header = new Header();
            this.results = new ObjectHypothesisWithPose[0];
            this.bbox = new BoundingBox2D();
            this.source_img = new Image();
        }

        public Detection2D(Header header, ObjectHypothesisWithPose[] results, BoundingBox2D bbox, Image source_img)
        {
            this.header = header;
            this.results = results;
            this.bbox = bbox;
            this.source_img = source_img;
        }
    }
}
