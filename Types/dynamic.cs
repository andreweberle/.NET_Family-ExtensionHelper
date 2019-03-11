using System;

namespace EbbsSoft.ExtensionHelpers.DynamicHelpers
{
    public static class utlis
    {
        /// <summary>
        /// Bubble Sort
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static dynamic BubbleSort(this int[] array)
        {
            try
            {
                // We will use the bubble sort algorthim
                for (int i = 0; i != array.Length; i++)
                {
                    for (int j = i + 1; j != array.Length; j++)
                    {
                        // if the current element is higher
                        // then the next element, we will swap
                        // their positions around.
                        if (array[i] > array[j])
                        {
                            EbbsSoft.ExtensionHelpers.VoidHelpers.Utils.Swap(ref array[i], ref array[j]);
                        }
                    }
                }

                // Once the algorithm is completed,
                // we will return the array back to
                // the caller.
                return array;
            }
            catch (Exception ex)
            {
                // If an exception is thrown,
                // the dynamic object given is most likely
                // unable to be sorted.
                throw new Exception(ex.Message);
            } 
        }
    }
}