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
    public class Classification3D : Message
    {
        public override string RosMessageName => "vision_msgs/Classification3D";

        //  Defines a 3D classification result.
        // 
        //  This result does not contain any position information. It is designed for
        //    classifiers, which simply provide probabilities given a source image.
        public Header header { get; set; }
        //  Class probabilities
        public ObjectHypothesis[] results { get; set; }
        //  The 3D data that generated these results (i.e. region proposal cropped out of
        //    the image). Not required for all detectors, so it may be empty.
        public PointCloud2 source_cloud { get; set; }

        public Classification3D()
        {
            this.header = new Header();
            this.results = new ObjectHypothesis[0];
            this.source_cloud = new PointCloud2();
        }

        public Classification3D(Header header, ObjectHypothesis[] results, PointCloud2 source_cloud)
        {
            this.header = header;
            this.results = results;
            this.source_cloud = source_cloud;
        }
    }
}
