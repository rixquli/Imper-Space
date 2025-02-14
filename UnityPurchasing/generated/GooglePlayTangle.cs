// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("BE++E3I1+1rHzuUPMJx8HNgZiO1vDFtdlZcQs3pYHL5/XSYqDdOgLPTA11sz1Bk0mILvFSjBzHinLpA6knIl07apoyiqqJHnI9YuvfiuY4gTUYMuv/Y19s/VMMIkoSCaL6G3nlU/PM9tbIdwn1HY1uohZQIz/Q6aE4qvJ94yFvw67JMit9SEcG5xLOikh5hCaDM3hP2AXgL9yzYwOdOIVMFz8NPB/Pf423e5dwb88PDw9PHy5hW1vi/m7ljDM1oH0nksDyMlWDUqsMv6texa/nwY+uTrwXAy/cAKHXNqIgd83wzQUEfQoOJH+0bsGLf863VEtOcJqH5mb3Pt+IQnE0RH8rZz8P7xwXPw+/Nz8PDxaUoNubqnfQWAcMusAS9ImvPy8PHw");
        private static int[] order = new int[] { 11,7,10,8,6,8,8,12,11,9,13,13,13,13,14 };
        private static int key = 241;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
