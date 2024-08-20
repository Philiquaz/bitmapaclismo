using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms.VisualStyles;

namespace bitmapaclismo
{
    enum MapSize : int
    {
        Small = 20,
        Medium = 30,
        Large = 40,
    }
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
        MapSize mapSize;
        public int sizeX;
        public int sizeY;
        public int sizeZ;
        int unk6; // 5
        int heightSize;
        int[] heightData;
        int mistSize;
        int[] mistData;
        int resourcesSize;
        ResourceType[] resourcesData;
        int groundTypeSize;
        GroundType[] groundTypeData;
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
            mapSize = (MapSize) data.readInt();
            sizeX = data.readInt();
            sizeY = data.readInt();
            sizeZ = data.readInt();
            unk6 = data.readInt();
            heightSize = data.readInt();
            heightData = data.readInts(heightSize);
            mistSize = data.readInt();
            mistData = data.readInts(mistSize);
            resourcesSize = data.readInt();
            resourcesData = data.readEnumBytes<ResourceType>(resourcesSize);
            groundTypeSize = data.readInt();
            groundTypeData = data.readEnumBytes<GroundType>(groundTypeSize);
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
            antiNameLength = -1 - newName.Length;
            nameLength = newName.Length;
            name = newName;
            unk4 = defaults.unk4;
            mapSize = defaults.mapSize;
            sizeX = x;
            sizeY = y;
            sizeZ = z;
            unk6 = defaults.unk6;
            heightSize = sizeX * sizeY;
            heightData = new int[heightSize];
            mistSize = sizeX * sizeY;
            mistData = new int[mistSize];
            resourcesSize = sizeX * sizeY;
            resourcesData = new ResourceType[resourcesSize];
            groundTypeSize = sizeX * sizeY;
            groundTypeData = new GroundType[groundTypeSize];
            monsterZoneCount = defaults.monsterZoneCount;
            monsterZones = new MonsterZone[monsterZoneCount];
            for (int i=0; i<monsterZoneCount; i++)
            {
                monsterZones[i] = defaults.monsterZones[i];
            }
            unk7 = new byte[defaults.unk7.Length];
            defaults.unk7.CopyTo(unk7, 0);
        }
        public void Write(ByteWriter data)
        {
            data.writeBytes(unk1);
            data.writeInt(antiNameLength);
            data.writeInt(nameLength);
            data.writeString(name);
            data.writeInt(unk4);
            data.writeInt((int)mapSize);
            data.writeInt(sizeX);
            data.writeInt(sizeY);
            data.writeInt(sizeZ);
            data.writeInt(unk6);
            data.writeInt(heightSize);
            data.writeInts(heightData);
            data.writeInt(mistSize);
            data.writeInts(mistData);
            data.writeInt(resourcesSize);
            data.writeEnumBytes<ResourceType>(resourcesData);
            data.writeInt(groundTypeSize);
            data.writeEnumBytes<GroundType>(groundTypeData);
            data.writeInt(monsterZoneCount);
            for (int i=0; i<monsterZoneCount; i++)
            {
                monsterZones[i].Write(data);
            }
            data.writeBytes(unk7);
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
        public ref int mist(int x, int y)
        {
            return ref mistData[x * sizeY + y];
        }
        public ref ResourceType resource(int x, int y)
        {
            return ref resourcesData[x * sizeY + y];
        }
        public ref GroundType groundType(int x, int y)
        {
            return ref groundTypeData[x * sizeY + y];
        }
        public ref MonsterZone MonsterZone(int i)
        {
            return ref monsterZones[i];
        }
    }

    public struct MonsterZone
    {
        //Who wrote this XZY when terrain file is XYZ? Or who wrote terrain file XZY when this is XYZ?
        //For clarity, bitmapaclismo uses xy horizontally and z for height.
        byte unk1;
        int id;
        int posX;
        int posZ;
        int posY;
        int sizeX;
        int sizeZ;
        int sizeY;
        
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
        public void Write(ByteWriter data)
        {
            data.writeByte(unk1);
            data.writeInt(id);
            data.writeInt(posX);
            data.writeInt(posZ);
            data.writeInt(posY);
            data.writeInt(sizeX);
            data.writeInt(sizeZ);
            data.writeInt(sizeY);
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