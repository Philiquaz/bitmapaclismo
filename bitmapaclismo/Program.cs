using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace bitmapaclismo
{
    static class bitmapaclismo
    {
        [STAThread]
        static void Main() {
            
            try {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Open level folder";
                fbd.UseDescriptionForTitle = true;
                fbd.ShowDialog();

                if (!Directory.Exists(fbd.SelectedPath)) {
                    MessageBox.Show("Could not find folder specified");
                    return;
                }
                if (!Path.Exists($@"{fbd.SelectedPath}\terrain")) {
                    MessageBox.Show("Could not find terrain file in folder specified");
                    return;
                }

                string fileName = Path.GetFileName(fbd.SelectedPath);
                byte[] terrainBytes = File.ReadAllBytes($@"{fbd.SelectedPath}\terrain");

                int sizex = Util.readInt(terrainBytes, 0x1E -5 + fileName.Length);
                int sizey = Util.readInt(terrainBytes, 0x22 -5 + fileName.Length);
                int sizez = Util.readInt(terrainBytes, 0x26 -5 + fileName.Length);
                int sizeL = Util.readInt(terrainBytes, 0x2A -5 + fileName.Length);
                int totalSize = Util.readInt(terrainBytes, 0x2E -5 + fileName.Length);
                int heightwidth = (int)Math.Sqrt(totalSize); //use sqrt for only square map because idk braindamage

                int zRange = 100; //Fixed
                if (heightwidth != sizex || heightwidth != sizey || zRange != sizez) {
                    MessageBox.Show("Unexpected size parameters in terrain file - was this already modified?");
                }

                if (MessageBox.Show("Choose your action:\n\nYes - Convert terrain file to a bitmap image.\nNo - Convert a bitmap image to the terrain file.", "Select Action", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    ConvertTerrainToBitmap(terrainBytes, fbd.SelectedPath, heightwidth);
                } else {
                    ConvertBitmapToTerrain(terrainBytes, fileName, fbd.SelectedPath, heightwidth, zRange, fbd);
                }
            } catch (Exception e)
            {
                MessageBox.Show("Unknown error: " + e.Message + "\n\n" + e.StackTrace);
            }
        }

        static void ConvertBitmapToTerrain(byte[] terrainBytes, string fileName, string folderPath, int heightwidth, int zRange, FolderBrowserDialog fbd) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = $@"Open {heightwidth}x{heightwidth}px 24bit bitmap";
            ofd.ShowDialog();
            if (!Path.Exists(ofd.FileName)) {
                MessageBox.Show("Could not find bmp file specified");
                return;
            }
            Bitmap bitmap = new Bitmap(ofd.FileName);

            if (MessageBox.Show("Extend height to 255 because you want to break the game?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                zRange = 255;
            }

            int terrainStart = 0x2d + fileName.Length;
            int terrainCurrent = terrainStart;

            for (int x=0; x<heightwidth; x++) {
                for (int y=0; y<heightwidth; y++) {
                    byte hMapValue = bitmap.GetPixel(x, y).R;
                    int result = (int)(hMapValue / 255f * zRange);
                    Util.writeInt(terrainBytes, terrainCurrent, result);
                    terrainCurrent += 4;
                }
            }
            File.WriteAllBytes($@"{fbd.SelectedPath}\terrain", terrainBytes);
            MessageBox.Show("Finished converting bitmap to terrain.");
        }
        
        static void ConvertTerrainToBitmap(byte[] terrainBytes, string folderPath, int heightwidth) {
            Bitmap bitmap = new Bitmap(heightwidth, heightwidth, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int terrainStart = 0x2d + Path.GetFileName(folderPath).Length;
            int terrainCurrent = terrainStart;

            int zRange = 100;

            for (int x = 0; x < heightwidth; x++) {
                for (int y = 0; y < heightwidth; y++) {
                    int heightValue = Util.readInt(terrainBytes, terrainCurrent);
                    float normalizedValue = heightValue / (float)zRange;
                    byte pixelValue = (byte)(normalizedValue * 255);

                    Color color = Color.FromArgb(pixelValue, pixelValue, pixelValue);
                    bitmap.SetPixel(x, y, color);

                    terrainCurrent += 4;
                }
            }

            string outputPath = Path.Combine(folderPath, "terrain.bmp");
            bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Bmp);
            MessageBox.Show($"Terrain converted to bitmap and saved as {outputPath}");
        }
    }
}