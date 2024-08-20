using System.Diagnostics.CodeAnalysis;

namespace bitmapaclismo
{
    class Terrain
    {
        byte[] unk1; // 17 in first byte
        int antiNameLength;
        int nameLength;
        String name;
        int unk4; // 0
        int unk5; // 40
        int sizeX;
        int sizeY;
        int sizeZ;
        int unk6; // 5
        int heightSize;
        int[] heightData;
        int unk7Size;
        int[] unk7Data;
        int unk8Size;
        byte[] unk8Data;
        int unk9Size;
        byte[] unk9Data;
        byte[] unk10;

        public Terrain(ByteReader data)
        {
            unk1 = data.readBytes(0x9);
            antiNameLength = data.readInt();
            nameLength = data.readInt();
            name = data.readString(nameLength);
            unk4 = data.readInt();
            unk5 = data.readInt();
            sizeX = data.readInt();
            sizeY = data.readInt();
            sizeZ = data.readInt();
            unk6 = data.readInt();
            heightSize = data.readInt();
            heightData = data.readInts(heightSize);
            unk7Size = data.readInt();
            unk7Data = data.readInts(unk7Size);
            unk8Size = data.readInt();
            unk8Data = data.readBytes(unk8Size);
            unk9Size = data.readInt();
            unk9Data = data.readBytes(unk9Size);
            unk10 = data.readBytes(-1);
        }

        public Terrain(Terrain defaults, String newName, int x, int y, int z)
        {
            unk1 = new byte[defaults.unk1.Length];
            defaults.unk1.CopyTo(unk1, 0);
            antiNameLength = defaults.antiNameLength;
            antiNameLength = defaults.nameLength;
            name = newName;
            unk4 = defaults.unk4;
            unk5 = defaults.unk5;
            sizeX = x;
            sizeY = y;
            sizeZ = z;
            unk6 = defaults.unk6;
            heightSize = sizeX * sizeY;
            heightData = new int[heightSize];
            unk7Size = sizeX * sizeY;
            unk7Data = new int[unk7Size];
            unk8Size = sizeX * sizeY;
            unk8Data = new byte[unk8Size];
            unk9Size = sizeX * sizeY;
            unk9Data = new byte[unk9Size];
            unk10 = defaults.unk10;
        }

        public bool Validate()
        {
            if (unk1.Length != 0x9)
                return false;
            if (unk1[0] != 17)
                return false;
            if (antiNameLength >= 0)
                return false;
            if (nameLength < 0)
                return false;
            if (antiNameLength != -1 - nameLength)
                return false;
            if (unk4 != 0)
                return false;
            if (unk5 != 40)
                return false;
            if (sizeX <= 0)
                return false;
            if (sizeY <= 0)
                return false;
            if (sizeZ <= 0)
                return false;
            if (unk6 != 5)
                return false;
            if (heightSize != sizeX * sizeY)
                return false;
            if (heightData.Length != heightSize)
                return false;
            if (unk7Size != sizeX * sizeY)
                return false;
            if (unk7Data.Length != unk7Size)
                return false;
            if (unk8Size != sizeX * sizeY)
                return false;
            if (unk8Data.Length != unk8Size)
                return false;
            if (unk9Size != sizeX * sizeY)
                return false;
            if (unk9Data.Length != unk9Size)
                return false;
            return true;
        }
        public ref int height(int x, int y)
        {
            return ref heightData[x * sizeY + y]; //arbitrary choice of y-major
        }
    }
}