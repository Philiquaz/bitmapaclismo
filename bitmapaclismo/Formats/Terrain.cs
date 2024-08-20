using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms.VisualStyles;

namespace bitmapaclismo
{
    enum ResourceType : byte
    {
        Empty = 0,
        Wood = 2,
        Stone = 1,
        Mineral = 3,
        Debris = 5

    }
    enum GroundType : byte
    {
        Normal = 0,
        Grime = 1
    }
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
        int mistSize;
        int[] mistData;
        int resourcesSize;
        byte[] resourcesData;
        int groundTypeSize;
        byte[] groundTypeData;
        int monsterZoneCount;
        MonsterZone[] monsterZones;
        byte[] unk7 = new byte[16];

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
            mistSize = data.readInt();
            mistData = data.readInts(mistSize);
            resourcesSize = data.readInt();
            resourcesData = data.readBytes(resourcesSize);
            groundTypeSize = data.readInt();
            groundTypeData = data.readBytes(groundTypeSize);
            monsterZoneCount = data.readInt();
            monsterZones = new MonsterZone[monsterZoneCount];
            for (int i=0; i < monsterZoneCount; i++)
            {
                monsterZones[i] = new MonsterZone(data);
            }
            unk7 = data.readBytes(16);
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
            mistSize = sizeX * sizeY;
            mistData = new int[mistSize];
            resourcesSize = sizeX * sizeY;
            resourcesData = new byte[resourcesSize];
            groundTypeSize = sizeX * sizeY;
            groundTypeData = new byte[groundTypeSize];
            monsterZoneCount = defaults.monsterZoneCount;
            monsterZones = new MonsterZone[monsterZoneCount];
            for (int i=0; i<monsterZoneCount; i++)
            {
                monsterZones[i] = defaults.monsterZones[i];
            }
            unk7 = new byte[defaults.unk7.Length];
            defaults.unk7.CopyTo(unk7, 0);
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
            if (mistSize != sizeX * sizeY)
                return false;
            if (mistData.Length != mistSize)
                return false;
            if (resourcesSize != sizeX * sizeY)
                return false;
            if (resourcesData.Length != resourcesSize)
                return false;
            if (groundTypeSize != sizeX * sizeY)
                return false;
            if (groundTypeData.Length != groundTypeSize)
                return false;
            if (monsterZoneCount < 0)
                return false;
            if (monsterZones.Length != monsterZoneCount)
                return false;
            for (int i=0; i<monsterZoneCount; i++)
            {
                if (!monsterZones[i].Validate(i))
                    return false;
            }
            if (unk7.Length != 16)
                return false;
            return true;
        }
        public ref int height(int x, int y)
        {
            return ref heightData[x * sizeY + y]; //arbitrary choice of y-major
        }
    }

    public struct MonsterZone
    {
        byte unk1 = 7;
        int id;
        int posX; //0
        int posZ; //1
        int posY; //0
        int sizeX; //1
        int sizeZ; //5
        int sizeY; //1
        
        public MonsterZone(ByteReader data)
        {
            unk1 = data.readByte();
            id = data.readInt();
            posX = data.readInt();
            posZ = data.readInt();
            posY = data.readInt();
            sizeX = data.readInt();
            sizeZ = data.readInt();
            sizeY = data.readInt();
        }
        public bool Validate(int index)
        {
            if (unk1 != 7)
                return false;
            if (id != index)
                return false;
            if (posX < 0)
                return false;
            if (posZ < 0)
                return false;
            if (posY < 0)
                return false;
            if (sizeX < 0)
                return false;
            if (sizeZ != 5)
                return false;
            if (sizeY < 0)
                return false;
            return true;
        }
    }
}