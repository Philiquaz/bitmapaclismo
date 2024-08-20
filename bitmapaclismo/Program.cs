using System;
using System.Drawing;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace bitmapaclismo
{
    static class bitmapaclismo
    {
        [STAThread]
        static void Main()
        {
            
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Open level folder";
                fbd.UseDescriptionForTitle = true;
                fbd.ShowDialog();

                if (!Directory.Exists(fbd.SelectedPath))
                {
                    MessageBox.Show("Could not find folder specified");
                    return;
                }
                if (!Path.Exists($@"{fbd.SelectedPath}\terrain"))
                {
                    MessageBox.Show("Could not find terrain file in folder specified");
                    return;
                }

                string fileName = Path.GetFileName(fbd.SelectedPath);
                byte[] terrainBytes = File.ReadAllBytes($@"{fbd.SelectedPath}\terrain");

                Terrain terrainFile = new Terrain(new ByteReader(terrainBytes));
                if (!terrainFile.Validate() && MessageBox.Show("Terrain file not recognised. Continue?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

                if (MessageBox.Show("Convert a bitmap image to the terrain file?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ConvertBitmapToTerrain(fbd.SelectedPath, fileName, terrainFile);
                }
                else if (MessageBox.Show("Convert terrain file to a bitmap image?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ConvertTerrainToBitmap(fbd.SelectedPath, terrainFile);
                }
                else
                {
                    MessageBox.Show("No option selected. Closing program.", "", MessageBoxButtons.OK);
                }
            } catch (Exception e)
            {
                MessageBox.Show("Unknown error: " + e.Message + "\n\n" + e.StackTrace);
            }
        }

        static void ConvertBitmapToTerrain(string folderPath, string fileName, Terrain terrain)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = $@"Open bitmap";
            ofd.ShowDialog();
            if (!Path.Exists(ofd.FileName)) {
                MessageBox.Show("Could not find bmp file specified");
                return;
            }
            Bitmap bitmap = new Bitmap(ofd.FileName);

            if (MessageBox.Show("Extend height to 255?\nThis will slow game load times much more than map size.", "", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                terrain.sizeZ = 255;
            }

            if (bitmap.Width != terrain.sizeX || bitmap.Height != terrain.sizeY)
            {

                if (MessageBox.Show("Bitmap is not size of map - map data will be wiped while clearing the map.", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
                if (bitmap.Width != bitmap.Height && MessageBox.Show("Bitmap is not square. Border region will be a square based on bitmap's width.\nContinue?", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
                terrain = new Terrain(terrain, fileName, bitmap.Width, bitmap.Height, terrain.sizeZ);
            }

            for (int x=0; x<terrain.sizeX; x++)
            {
                for (int y=0; y<terrain.sizeY; y++)
                {
                    byte hMapValue = bitmap.GetPixel(x, y).R;
                    int result = (int)(hMapValue / 255f * terrain.sizeZ);
                    terrain.height(x, y) = result;
                }
            }
            ByteWriter bw = new ByteWriter();
            terrain.Write(bw);
            File.WriteAllBytes($@"{folderPath}\terrain", bw.GetBytes());
        }
        
        static void ConvertTerrainToBitmap(string folderPath, Terrain terrain)
        {
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = $@"Save bitmap";
            sfd.DefaultExt = "bmp";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            Bitmap bitmap = new Bitmap(terrain.sizeX, terrain.sizeY, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            bool includeLayers = false;
            if (MessageBox.Show("Include resource (G), ground type (G) and mist (B) layers?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                includeLayers = true;

            for (int x = 0; x < terrain.sizeX; x++)
            {
                for (int y = 0; y < terrain.sizeY; y++)
                {
                    byte heightValue = (byte)(terrain.height(x, y) * 255f / terrain.sizeZ);
                    Color color;
                    if (includeLayers)
                    {
                        
                        byte resAndGround = (byte)((byte)terrain.resource(x, y) * 46 - (byte)terrain.groundType(x, y) * 23);
                        byte mist = (byte)(terrain.mist(x, y) * 255f / terrain.sizeZ);
                        color = Color.FromArgb(heightValue, resAndGround, mist);
                    }
                    else
                    {
                        color = Color.FromArgb(heightValue, heightValue, heightValue);
                    }
                    bitmap.SetPixel(x, y, color);
                }
            }
            bitmap.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}