using System;
using System.Collections.Generic;
using System.IO;

namespace ViterbiTracking
{
    class Program
    {

        /// <summary>
        /// Sample program for the Viterbi class
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // generate sample data with random objects
            List<List<TrackedObject>> objectsByWorldState = sampleData(3, 4);
            ViterbiTracking viterbi = new ViterbiTracking();

            // process the viterbi algorithm, obtaining the paths (as strings)
            string paths = viterbi.process(objectsByWorldState);
            Console.WriteLine("=== PATHS ================================");
            Console.WriteLine(paths);

            // display the matrix details
            Console.WriteLine("=== DETAILS ==============================");
            string matrix_output = viterbi.displayMatrix(objectsByWorldState);
            Console.WriteLine(matrix_output);

            // wait for ENTER key to close the console
            Console.ReadLine();            
        }

        
        /// <summary>
        /// Generates random sample data: k objects by n world states
        /// </summary>
        /// <param name="kObjects">Quantity of objects to create by world staet</param>
        /// <param name="nWorldStates">Quantity of world states</param>
        /// <returns>"Matrix" of objects by worldstate</returns>
        public static List<List<TrackedObject>> sampleData(int kObjects, int nWorldStates)
        {

            // the "matrix" of objects by world state
            List<List<TrackedObject>> objectsByWorldState = new List<List<TrackedObject>>();

            // for this example, the objects will be at random posiions
            // Generate random locations for K x N points
            var rnd = new Random(DateTime.Now.Millisecond);
            for (int iWorldState = 0; iWorldState < nWorldStates; iWorldState++)
            {
                List<TrackedObject> objectsAtWorldState = new List<TrackedObject>();
                for (int iObject = 0; iObject < kObjects; iObject++)
                {
                    int x = rnd.Next(0, 100);
                    int y = rnd.Next(0, 100);
                    //TrackedObject thisObject = new TrackedObject(x, y);
                    TrackedObject thisObject = new TrackedObject(x, y, iObject);

                    // add the object to the objects list at this world state
                    objectsAtWorldState.Add(thisObject);
                }

                // add the list of objects in this world state to the bidimensional matrix
                objectsByWorldState.Add(objectsAtWorldState);
            }

            return objectsByWorldState;
        }

    }

}