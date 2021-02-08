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
    public class Classification2D : Message
    {
        public override string RosMessageName => "vision_msgs/Classification2D";

        //  Defines a 2D classification result.
        // 
        //  This result does not contain any position information. It is designed for
        //    classifiers, which simply provide class probabilities given a source image.
        public Header header { get; set; }
        //  A list of class probabilities. This list need not provide a probability for
        //    every possible class, just ones that are nonzero, or above some
        //    user-defined threshold.
        public ObjectHypothesis[] results { get; set; }
        //  The 2D data that generated these results (i.e. region proposal cropped out of
        //    the image). Not required for all use cases, so it may be empty.
        public Image source_img { get; set; }

        public Classification2D()
        {
            this.header = new Header();
            this.results = new ObjectHypothesis[0];
            this.source_img = new Image();
        }

        public Classification2D(Header header, ObjectHypothesis[] results, Image source_img)
        {
            this.header = header;
            this.results = results;
            this.source_img = source_img;
        }
    }
}
