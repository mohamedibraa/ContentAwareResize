using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace ContentAwareResize
{
    // *****************************************
    // DON'T CHANGE CLASS OR FUNCTION NAME
    // YOU CAN ADD FUNCTIONS IF YOU NEED TO
    // *****************************************
    public class ContentAwareResize
    {
        public struct coord
        {
            public int row;
            public int column;
        }
        //========================================================================================================
        //Your Code is Here:
        //===================
        /// <summary>
        /// Develop an efficient algorithm to get the minimum vertical seam to be removed
        /// </summary>
        /// <param name="energyMatrix">2D matrix filled with the calculated energy for each pixel in the image</param>
        /// <param name="Width">Image's width</param>
        /// <param name="Height">Image's height</param>
        /// <returns>BY REFERENCE: The min total value (energy) of the selected seam in "minSeamValue" & List of points of the selected min vertical seam in seamPathCoord</returns>
        public void CalculateSeamsCost(int[,] energyMatrix, int Width, int Height, ref int minSeamValue, ref List<coord> seamPathCoord)
        {
            int[,] R = new int[energyMatrix.GetLength(0), energyMatrix.GetLength(1)];
            int []x = new int[5];
            int y=x.GetLength(0);
            Array.Sort(x);
            List<coord> pointslist = new List<coord>();
            coord obj = new coord();

            //code with building table method to getmin seam value

            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    if (j == 0)
                        R[j, i] = energyMatrix[j, i];

                    else if (i == 0)
                    {
                        int b = R[j - 1, i];
                        int c = R[j - 1, i + 1];
                        R[j, i] = energyMatrix[j, i] + Math.Min(b, c);
                    }
                    else if (i == Width - 1)
                    {
                        int a = R[j - 1, i - 1];
                        int b = R[j - 1, i];
                        R[j, i] = energyMatrix[j, i] + Math.Min(b, a);
                    }
                    else
                    {
                        int a = R[j - 1, i - 1];
                        int b = R[j - 1, i];
                        int c = R[j - 1, i + 1];
                        R[j, i] = energyMatrix[j, i] + Math.Min(Math.Min(a, b), c);
                    }

                }
            }

            // lopp in last raw in matrix R to get minvalue and save col index of this minvalue
            minSeamValue = R[Height - 1, Width - 1];
            int columnindex = 0;
            for (int i = Width; i > 0; i--)
            {
                if (minSeamValue > R[Height - 1, i - 1])
                {
                    minSeamValue = R[Height - 1, i - 1];
                    columnindex = i - 1;
                }
            }

            // print func that to get point of min path start from last row and col of minvalue
            List<coord> printpath(int j, int i)
            {
                obj.row = j;
                obj.column = i;
                pointslist.Add(obj);

                if (j == 0)
                    return pointslist;

                else if (i == 0)
                {
                    int b = energyMatrix[j - 1, i];
                    int c = energyMatrix[j - 1, i + 1];

                    if (b < c)
                        return printpath(j - 1, i);
                    else
                        return printpath(j - 1, i + 1);
                }
                else if (i == Width - 1)
                {
                    int a = energyMatrix[j - 1, i - 1];
                    int b = energyMatrix[j - 1, i];

                    if (b < a)
                        return printpath(j - 1, i);
                    else
                        return (printpath(j - 1, i - 1));
                }
                else
                {
                    int a = energyMatrix[j - 1, i - 1];
                    int b = energyMatrix[j - 1, i];
                    int c = energyMatrix[j - 1, i + 1];

                    if (b < a && b < c)
                        return printpath(j - 1, i);
                    else if (a < b && a < c)
                        return printpath(j - 1, i - 1);
                    else
                        return printpath(j - 1, i + 1);
                }
            }
            printpath(Height - 1, columnindex); // call function start from last row and col of minvalue
            seamPathCoord = pointslist;
        }

        // *****************************************
        // DON'T CHANGE CLASS OR FUNCTION NAME
        // YOU CAN ADD FUNCTIONS IF YOU NEED TO 
        // *****************************************
        #region DON'TCHANGETHISCODE
        public MyColor[,] _imageMatrix;
        public int[,] _energyMatrix;
        public int[,] _verIndexMap;
        public ContentAwareResize(string ImagePath)
        {
            _imageMatrix = ImageOperations.OpenImage(ImagePath);
            _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);
            int _height = _energyMatrix.GetLength(0);
            int _width = _energyMatrix.GetLength(1);
        }
        public void CalculateVerIndexMap(int NumberOfSeams, ref int minSeamValueFinal, ref List<coord> seamPathCoord)
        {
            int Width = _imageMatrix.GetLength(1);
            int Height = _imageMatrix.GetLength(0);

            int minSeamValue = -1;
            _verIndexMap = new int[Height, Width];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    _verIndexMap[i, j] = int.MaxValue;

            bool[] RemovedSeams = new bool[Width];
            for (int j = 0; j < Width; j++)
                RemovedSeams[j] = false;

            for (int s = 1; s <= NumberOfSeams; s++)
            {
                CalculateSeamsCost(_energyMatrix, Width, Height, ref minSeamValue, ref seamPathCoord);
                minSeamValueFinal = minSeamValue;

                //Search for Min Seam # s
                int Min = minSeamValue;

                //Mark all pixels of the current min Seam in the VerIndexMap
                if (seamPathCoord.Count != Height)
                    throw new Exception("You selected WRONG SEAM");
                for (int i = Height - 1; i >= 0; i--)
                {
                    if (_verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] != int.MaxValue)
                    {
                        string msg = "overalpped seams between seam # " + s + " and seam # " + _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column];
                        throw new Exception(msg);
                    }
                    _verIndexMap[seamPathCoord[i].row, seamPathCoord[i].column] = s;
                    //remove this seam from energy matrix by setting it to max value
                    _energyMatrix[seamPathCoord[i].row, seamPathCoord[i].column] = 100000;
                }

                //re-calculate Seams Cost in the next iteration again
            }
        }
        public void RemoveColumns(int NumberOfCols)
        {
            int Width = _imageMatrix.GetLength(1);
            int Height = _imageMatrix.GetLength(0);
            _energyMatrix = ImageOperations.CalculateEnergy(_imageMatrix);

            int minSeamValue = 0;
            List<coord> seamPathCoord = null;
            //CalculateSeamsCost(_energyMatrix,Width,Height,ref minSeamValue, ref seamPathCoord);
            CalculateVerIndexMap(NumberOfCols, ref minSeamValue, ref seamPathCoord);

            MyColor[,] OldImage = _imageMatrix;
            _imageMatrix = new MyColor[Height, Width - NumberOfCols];
            for (int i = 0; i < Height; i++)
            {
                int cnt = 0;
                for (int j = 0; j < Width; j++)
                {
                    if (_verIndexMap[i, j] == int.MaxValue)
                        _imageMatrix[i, cnt++] = OldImage[i, j];
                }
            }

        }
        #endregion
    }
}
