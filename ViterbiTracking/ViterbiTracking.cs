using System;
using System.Collections.Generic;

namespace ViterbiTracking
{
   
    class ViterbiTracking
    {

        // Cost relationship between points of adjacent world state
        public int[,,] pairwiseCost;

        // structure to encapsulate a given source object and its cost
        struct ObjectAndCost
        {
            public int cost;
            public TrackedObject sourceObject;
        }

        
        /// <summary>
        /// Analyse the objects by worldstate, assigning their associated cost based on their distance
        /// </summary>
        /// <param name="objectsByWorldState"></param>
        public string process(List<List<TrackedObject>> objectsByWorldState)
        {

            // Cost relationship between points of adjacent world state
            calculatePairwisteCost(objectsByWorldState);

            // FORWARD PROCESSING: Calculate the minimum cumulative cost for each object in each world state
            calculateMinimumCumulativeCost(objectsByWorldState);

            // BACKWARD PROCESSING:
            return getPaths(objectsByWorldState);

        }


        /// <summary>
        /// Return the dependent shortest path for each object (as a string)
        /// </summary>
        /// <param name="objectsByWorldState">"Matrix" of objects by worldstate</param>
        /// <returns>Paths (as string)</returns>
        public string getPaths(List<List<TrackedObject>> objectsByWorldState)
        {
            string output = "";

            // sort the elments by ascending cost
            objectsByWorldState[objectsByWorldState.Count - 1].Sort(delegate (TrackedObject c1, TrackedObject c2) { return c1.cost.CompareTo(c2.cost); });
            
            foreach (TrackedObject objectAtWorldState in objectsByWorldState[objectsByWorldState.Count - 1]) 
            {
                int iWorldState = objectsByWorldState.Count;
                // temporary object to iterate through the linked list
                TrackedObject thisObject = objectAtWorldState;                
                do
                {
                    iWorldState--;
                    output += "(" + thisObject.x + "," + thisObject.y + ") ["+ thisObject.cost + "]";

                    // if there is no closest point from previous world state, then we have reached the end
                    if (thisObject.closestSource == null) break;

                    // make sure to not using source objects already used by other shorter paths
                    if (thisObject.closestSource.taken)
                    {
                        // if so, find the next one that is closer
                        ObjectAndCost closestSource = minCost(objectsByWorldState[iWorldState - 1], iWorldState, thisObject.ith);
                        thisObject.closestSource = closestSource.sourceObject;

                        // if there is no closest point from previous world state, then we have reached the end
                        if (thisObject == null)
                            break;               
                    }

                    // now we move in our liked list, to the previous object (a.k.a. the closest one from previous world state)
                    thisObject = thisObject.closestSource;

                    if (thisObject != null)
                    {
                        // mark the object as taken, so it is used only once.
                        thisObject.taken = true;
                        output += " -> ";
                    }
                } while (thisObject != null);
                
                output += Environment.NewLine;
            }

            return output;
        }


        /// <summary>
        /// Displays the "Matrix" of objects by worldstate
        /// </summary>
        /// <param name="objectsByWorldState">"Matrix" of objects by worldstate</param>
        /// <returns>String</returns>
        public string displayMatrix(List<List<TrackedObject>> objectsByWorldState)
        {
            string output = "";
            int iWorldState = 0;
            foreach (List<TrackedObject> objectsAtWorldState in objectsByWorldState)
            {
                output += "--- WorldState " + iWorldState + " ---" + Environment.NewLine;
                int iDestinyObject = 0;
                foreach (TrackedObject destinyObject in objectsAtWorldState)
                {
                    // display object coordinates
                    output += "(" + destinyObject.x + "," + destinyObject.y + ":" + destinyObject.cost + ")\t\t";

                    // display cost to this object, coming from a previous object
                    if (iWorldState > 0)
                    {
                        // get the objects from previous world state
                        List<TrackedObject> objectsAtPreviousWorldState = objectsByWorldState[iWorldState - 1];
                        int iSourceObject = 0;
                        foreach (TrackedObject sourceObject in objectsAtPreviousWorldState)
                        {
                            int distance = this.pairwiseCost[iWorldState, iDestinyObject, iSourceObject];
                            output += "[" + distance + "]";
                            iSourceObject++;
                        }
                    }
                    output += Environment.NewLine;

                    iDestinyObject++;
                }
                output += Environment.NewLine;
                iWorldState++;
            }
            output += Environment.NewLine;
            return output;
        }


        /// <summary>
        /// // FORWARD PROCESSING: Calculate the minimum cumulative cost for each object in each world state
        /// </summary>
        /// <param name="objectsByWorldState">"Matrix" of objects by world state</param>
        private void calculateMinimumCumulativeCost(List<List<TrackedObject>> objectsByWorldState)
        {            
            int iWorldState = 0;
            foreach (List<TrackedObject> objectsAtWorldState in objectsByWorldState)
            {
                // we start ecalculating costs from the second world stat
                if (iWorldState == 0)
                {
                    iWorldState++;
                    continue;
                }

                int iDestinyObject = 0;
                foreach (TrackedObject destinyObject in objectsAtWorldState)
                {
                    //destinyObject.cost = minCost(objectsByWorldState[iWorldState - 1], iWorldState, iDestinyObject);
                    ObjectAndCost minSourceObject = minCost(objectsByWorldState[iWorldState - 1], iWorldState, iDestinyObject);
                    destinyObject.cost = minSourceObject.cost;
                    
                    // set the object as a "tentative" closest source. It may be used in the path of another object having
                    // a total smaller path than this
                    destinyObject.closestSource = minSourceObject.sourceObject;
                    iDestinyObject++;
                }
                iWorldState++;
            }
        }

        /// <summary>
        /// Calculates the cost between pair of objects in adjacent world states (i.e. combination of source with destiny objects)
        /// </summary>
        /// <param name="objectsByWorldState">"Matrix" of objects by world state</param>
        private void calculatePairwisteCost(List<List<TrackedObject>> objectsByWorldState)
        {
            int dX, dY, distance;

            // get the maximum number of objects detected in any world state
            int nMaxObjects = getMaxObjectsInWorldStates(objectsByWorldState);

            int nWorldStates = objectsByWorldState.Count;
            pairwiseCost = new int[nWorldStates, nMaxObjects, nMaxObjects];
            int iWorldState = 0;
            foreach (List<TrackedObject> objectsAtWorldState in objectsByWorldState)
            {
                // costs begin from second world state (connecting to the first one)
                if (iWorldState == 0)
                {
                    iWorldState++;
                    continue;
                }

                // for each object in this world state
                int iDestinyObject = 0;
                foreach (TrackedObject destinyObject in objectsAtWorldState)
                {
                    // get the objects from previous world state
                    List<TrackedObject> objectsAtPreviousWorldState = objectsByWorldState[iWorldState - 1];
                    int iSourceObject = 0;
                    foreach (TrackedObject sourceObject in objectsAtPreviousWorldState)
                    {
                        // calculate the distance (cost) to this obect from an object from previous world state
                        dX = destinyObject.x - sourceObject.x;
                        dY = destinyObject.y - sourceObject.y;
                        distance = dX * dX + dY * dY;
                        pairwiseCost[iWorldState, iDestinyObject, iSourceObject] = distance;
                        iSourceObject++;
                    }
                    iDestinyObject++;
                }
                iWorldState++;
            }
        }


        /// <summary>
        /// Returns the ith position of the object with shortest path at the given world state
        /// </summary>
        /// <param name="objectsAtWorldState">List of tracked objects at the given world state</param>
        /// <returns>ith position of the object with shortest path</returns>
        private int objectWithShortestPathAtWorldState(List<TrackedObject> objectsAtWorldState) {
            int smallestPath = 99999999;
            int iCheaperObject = 0;
            for(int iObj=0; iObj< objectsAtWorldState.Count; iObj++)
            {
                if (smallestPath> objectsAtWorldState[iObj].cost)
                {
                    smallestPath = objectsAtWorldState[iObj].cost;
                    iCheaperObject = iObj;
                }
            }
            return iCheaperObject;
        }

        /// <summary>
        /// Returns the object source (i.e. from previous world state) that generates the smaller cost to reach this object
        /// in the current world state
        /// </summary>
        /// <param name="objectsAtPreviousWorldState"></param>
        /// <param name="iWorldState"></param>
        /// <param name="iObject"></param>
        /// <returns></returns>
        private ObjectAndCost minCost(List<TrackedObject> objectsAtPreviousWorldState, int iWorldState, int iObject)
        {
            int minDistance = 9999999;
            ObjectAndCost minObject;

            minObject.cost = 0;
            minObject.sourceObject = null;

            if (iWorldState == 0) return minObject;

            int iSourceObject = 0;
            foreach (TrackedObject thisObject in objectsAtPreviousWorldState)
            {
                // if the tentative source object has not been taken and it has the shortest distance, 
                // then replace the minObject with it
                if ((!thisObject.taken) && (minDistance > pairwiseCost[iWorldState, iObject, iSourceObject] + thisObject.cost))
                {
                    minDistance = pairwiseCost[iWorldState, iObject, iSourceObject] + thisObject.cost;
                    minObject.cost = minDistance;
                    minObject.sourceObject = thisObject;
                }
                iSourceObject++;
            }
            return minObject;
        }


        /// <summary>
        /// get the maximum number of objects detected in any world state
        /// </summary>
        /// <param name="objectsByWorldState">Matrix of objects by world state</param>
        /// <returns>max number of objects to handle by world state</returns>
        private int getMaxObjectsInWorldStates(List<List<TrackedObject>> objectsByWorldState)
        {
            int max = 0;
            foreach (List<TrackedObject> objectsAtWorldState in objectsByWorldState)
            {
                max = max < objectsAtWorldState.Count ? objectsAtWorldState.Count : max;
            }
            return max;
        }

        
    }
}
