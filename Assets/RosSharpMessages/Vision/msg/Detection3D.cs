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
    public class Detection3D : Message
    {
        public override string RosMessageName => "vision_msgs/Detection3D";

        //  Defines a 3D detection result.
        // 
        //  This extends a basic 3D classification by including position information,
        //    allowing a classification result for a specific position in an image to
        //    to be located in the larger image.
        public Header header { get; set; }
        //  Class probabilities. Does not have to include hypotheses for all possible
        //    object ids, the scores for any ids not listed are assumed to be 0.
        public ObjectHypothesisWithPose[] results { get; set; }
        //  3D bounding box surrounding the object.
        public BoundingBox3D bbox { get; set; }
        //  The 3D data that generated these results (i.e. region proposal cropped out of
        //    the image). This information is not required for all detectors, so it may
        //    be empty.
        public PointCloud2 source_cloud { get; set; }

        public Detection3D()
        {
            this.header = new Header();
            this.results = new ObjectHypothesisWithPose[0];
            this.bbox = new BoundingBox3D();
            this.source_cloud = new PointCloud2();
        }

        public Detection3D(Header header, ObjectHypothesisWithPose[] results, BoundingBox3D bbox, PointCloud2 source_cloud)
        {
            this.header = header;
            this.results = results;
            this.bbox = bbox;
            this.source_cloud = source_cloud;
        }
    }
}
