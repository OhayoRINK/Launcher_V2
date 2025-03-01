using System;
using System.IO;
using KartLibrary.File;
using KartLibrary.Consts;
using System.Collections;
using System.Collections.Generic;
using KartLibrary.Xml;
using System.Text;
using ExcData;
using KartRider.Common.Utilities;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace RHOParser
{
    public static class KartRhoFile
    {
        public static PackFolderManager packFolderManager = new PackFolderManager();

        public static void Dump(string input)
        {
            packFolderManager.OpenDataFolder(input);
            string regionCode = packFolderManager.regionCode.ToString().ToLower();
            Queue<PackFolderInfo> packFolderInfoQueue = new Queue<PackFolderInfo>();
            packFolderInfoQueue.Enqueue(packFolderManager.GetRootFolder());
            packFolderManager.GetRootFolder();
            while (packFolderInfoQueue.Count > 0)
            {
                PackFolderInfo packFolderInfo1 = packFolderInfoQueue.Dequeue();
                foreach (PackFileInfo packFileInfo in packFolderInfo1.GetFilesInfo())
                {
                    string fullName = ReplacePath(packFileInfo.FullName);
                    if (fullName.Contains("flyingPet") && fullName.Contains("param@" + regionCode + ".bml"))
                    {
                        Console.WriteLine(fullName);
                        string name = fullName.Substring(10, fullName.Length - 23);
                        if (!(KartExcData.flyingSpec.ContainsKey(name)))
                        {
                            byte[] data = packFileInfo.GetData();
                            BinaryXmlDocument bxd = new BinaryXmlDocument();
                            bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                            string output_bml = bxd.RootTag.ToString();
                            byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output_bml);
                            using (MemoryStream stream = new MemoryStream(output_data))
                            {
                                XmlDocument flying = new XmlDocument();
                                flying.Load(stream);
                                KartExcData.flyingSpec.Add(name, flying);
                            }
                        }
                    }
                    if (fullName == "track/common/randomTrack@" + regionCode + ".bml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        BinaryXmlDocument bxd = new BinaryXmlDocument();
                        bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                        string output_bml = bxd.RootTag.ToString();
                        byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output_bml);
                        using (MemoryStream stream = new MemoryStream(output_data))
                        {
                            KartExcData.randomTrack = XDocument.Load(stream);
                        }
                    }
                    if (fullName == "track/common/trackLocale@" + regionCode + ".xml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            XmlDocument trackLocale = new XmlDocument();
                            trackLocale.Load(stream);
                            XmlNodeList trackParams = trackLocale.GetElementsByTagName("track");
                            if (trackParams.Count > 0)
                            {
                                foreach (XmlNode xn in trackParams)
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    string track = xe.GetAttribute("id");
                                    uint id = Adler32Helper.GenerateAdler32_UNICODE(track, 0);
                                    if (!(KartExcData.track.ContainsKey(id)))
                                    {
                                        KartExcData.track.Add(id, track);
                                    }
                                }
                            }
                        }
                    }
                    if (fullName == "track/common/trackLocale@" + regionCode + ".bml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        BinaryXmlDocument bxd = new BinaryXmlDocument();
                        bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                        string output_bml = bxd.RootTag.ToString();
                        byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output_bml);
                        using (MemoryStream stream = new MemoryStream(output_data))
                        {
                            XmlDocument trackLocale = new XmlDocument();
                            trackLocale.Load(stream);
                            XmlNodeList trackParams = trackLocale.GetElementsByTagName("track");
                            if (trackParams.Count > 0)
                            {
                                foreach (XmlNode xn in trackParams)
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    string track = xe.GetAttribute("id");
                                    uint id = Adler32Helper.GenerateAdler32_UNICODE(track, 0);
                                    if (!(KartExcData.track.ContainsKey(id)))
                                    {
                                        KartExcData.track.Add(id, track);
                                    }
                                }
                            }
                        }
                    }
                    if (fullName == "etc_/itemTable.kml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            XDocument doc = XDocument.Load(stream);

                            var kartsWithName = doc.Descendants("kart").Where(kart => kart.Attribute("name") != null);
                            if (kartsWithName.Count() > 0)
                            {
                                foreach (var kart in kartsWithName)
                                {
                                    int id = int.Parse(kart.Attribute("id").Value);
                                    string name = kart.Attribute("name").Value;
                                    if (!(KartExcData.KartName.ContainsKey(id)))
                                    {
                                        KartExcData.KartName.Add(id, name);
                                    }
                                }
                            }

                            var flyingWithName = doc.Descendants("flyingPet").Where(kart => kart.Attribute("name") != null);
                            if (flyingWithName.Count() > 0)
                            {
                                foreach (var flyingPet in flyingWithName)
                                {
                                    int id = int.Parse(flyingPet.Attribute("id").Value);
                                    string name = flyingPet.Attribute("name").Value;
                                    if (!(KartExcData.flyingName.ContainsKey(id)))
                                    {
                                        KartExcData.flyingName.Add(id, name);
                                    }
                                }
                            }
                        }
                    }
                    if (fullName == "etc_/itemTable@" + regionCode + ".xml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            XDocument doc = XDocument.Load(stream);
                            var kartsWithName = doc.Descendants("kart").Where(kart => kart.Attribute("name") != null);
                            if (kartsWithName.Count() > 0)
                            {
                                foreach (var kart in kartsWithName)
                                {
                                    int id = int.Parse(kart.Attribute("id").Value);
                                    string name = kart.Attribute("name").Value;
                                    if (KartExcData.KartName.ContainsKey(id))
                                    {
                                        KartExcData.KartName[id] = name;
                                    }
                                    else
                                    {
                                        KartExcData.KartName.Add(id, name);
                                    }
                                }
                            }
                            var flyingsWithName = doc.Descendants("flyingPet").Where(flyingPet => flyingPet.Attribute("name") != null);
                            if (flyingsWithName.Count() > 0)
                            {
                                foreach (var flyingPet in flyingsWithName)
                                {
                                    int id = int.Parse(flyingPet.Attribute("id").Value);
                                    string name = flyingPet.Attribute("name").Value;
                                    if (KartExcData.flyingName.ContainsKey(id))
                                    {
                                        KartExcData.flyingName[id] = name;
                                    }
                                    else
                                    {
                                        KartExcData.flyingName.Add(id, name);
                                    }
                                }
                            }
                        }
                    }
                    if (fullName == "etc_/emblem/emblem@" + regionCode + ".xml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(stream);
                            XmlNodeList bodyParams = doc.GetElementsByTagName("emblem");
                            if (bodyParams.Count > 0)
                            {
                                foreach (XmlNode xn in bodyParams)
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    short id;
                                    if (short.TryParse(xe.GetAttribute("id"), out id))
                                    {
                                        if (!KartExcData.emblem.Contains(id))
                                        {
                                            KartExcData.emblem.Add(id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (fullName.Contains("kart_") && fullName.Contains("/param@" + regionCode + ".xml"))
                    {
                        Console.WriteLine(fullName);
                        byte[] data = ReplaceBytes(packFileInfo.GetData());
                        string name = fullName.Substring(6, fullName.Length - 19);
                        if (!(KartExcData.KartSpec.ContainsKey(name)))
                        {
                            if (data[2] == 13 && data[3] == 0 && data[4] == 10 && data[5] == 0)
                            {
                                byte[] newBytes = new byte[data.Length - 4];
                                newBytes[0] = 255;
                                newBytes[1] = 254;
                                Array.Copy(data, 6, newBytes, 2, data.Length - 6);
                                using (MemoryStream stream = new MemoryStream(newBytes))
                                {
                                    XmlDocument kart1 = new XmlDocument();
                                    kart1.Load(stream);
                                    KartExcData.KartSpec.Add(name, kart1);
                                }
                            }
                            else
                            {
                                using (MemoryStream stream = new MemoryStream(data))
                                {
                                    XmlDocument kart2 = new XmlDocument();
                                    kart2.Load(stream);
                                    KartExcData.KartSpec.Add(name, kart2);
                                }
                            }
                        }
                    }
                    if (fullName.Contains("kart_") && fullName.Contains("/param.xml"))
                    {
                        string name = fullName.Substring(6, fullName.Length - 16);
                        byte[] data = ReplaceBytes(packFileInfo.GetData());
                        bool containsTarget = packFolderInfo1.GetFilesInfo().Any(PackFileInfo => ReplacePath(PackFileInfo.FullName) == "kart_/" + name + "/param@" + regionCode + ".xml");
                        if (!containsTarget)
                        {
                            if (!(KartExcData.KartSpec.ContainsKey(name)))
                            {
                                Console.WriteLine(fullName);
                                if (data[2] == 13 && data[3] == 0 && data[4] == 10 && data[5] == 0)
                                {
                                    byte[] newBytes = new byte[data.Length - 4];
                                    newBytes[0] = 255;
                                    newBytes[1] = 254;
                                    Array.Copy(data, 6, newBytes, 2, data.Length - 6);
                                    using (MemoryStream stream = new MemoryStream(newBytes))
                                    {
                                        XmlDocument kart1 = new XmlDocument();
                                        kart1.Load(stream);
                                        KartExcData.KartSpec.Add(name, kart1);
                                    }
                                }
                                else
                                {
                                    using (MemoryStream stream = new MemoryStream(data))
                                    {
                                        XmlDocument kart2 = new XmlDocument();
                                        kart2.Load(stream);
                                        KartExcData.KartSpec.Add(name, kart2);
                                    }
                                }
                            }
                        }
                    }
                    if (fullName == "zeta/" + regionCode + "/quest/QuestAutomation.bml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        BinaryXmlDocument bxd = new BinaryXmlDocument();
                        bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                        string output_bml = bxd.RootTag.ToString();
                        byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output_bml);
                        using (MemoryStream stream = new MemoryStream(output_data))
                        {
                            XmlDocument Quest = new XmlDocument();
                            Quest.Load(stream);
                            XmlNodeList QuestParams = Quest.GetElementsByTagName("QuestItem");
                            if (QuestParams.Count > 0)
                            {
                                foreach (XmlNode xn in QuestParams)
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    int id = int.Parse(xe.GetAttribute("id"));
                                    if (!(KartExcData.quest.Contains(id)))
                                    {
                                        KartExcData.quest.Add(id);
                                    }
                                }
                            }
                        }
                    }
                    if (fullName == "zeta/" + regionCode + "/scenario/scenario.bml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        BinaryXmlDocument bxd = new BinaryXmlDocument();
                        bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                        string output_bml = bxd.RootTag.ToString();
                        byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output_bml);
                        using (MemoryStream stream = new MemoryStream(output_data))
                        {
                            XmlDocument Scenario = new XmlDocument();
                            Scenario.Load(stream);
                            XmlNodeList ScenarioParams = Scenario.GetElementsByTagName("Chapter");
                            if (ScenarioParams.Count > 0)
                            {
                                foreach (XmlNode xn in ScenarioParams)
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    int id = int.Parse(xe.GetAttribute("id"));
                                    if (!(KartExcData.scenario.Contains(id)))
                                    {
                                        KartExcData.scenario.Add(id);
                                    }
                                }
                            }
                        }
                    }
                    if (fullName == "zeta_/" + regionCode + "/content/itemDictionary.xml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            XDocument doc = XDocument.Load(stream);
                            var items = doc.Descendants("item");
                            foreach (var item in items)
                            {
                                short catId = short.Parse(item.Attribute("catId")?.Value ?? "0");
                                string valuesStr = item.Attribute("values")?.Value;
                                string[] valuesArray = valuesStr?.Split(',');
                                for (int i = 0; i < valuesArray.Count(); i++)
                                {
                                    List<short> Add = new List<short> { catId, short.Parse(valuesArray[i]) };
                                    KartExcData.Dictionary.Add(Add);
                                }
                            }
                        }
                    }
                    if (fullName == "zeta_/" + regionCode + "/shop/data/item.kml")
                    {
                        Console.WriteLine(fullName);
                        byte[] data = packFileInfo.GetData();
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(stream);
                            XmlNodeList bodyParams = doc.GetElementsByTagName("item");
                            if (bodyParams.Count > 0)
                            {
                                foreach (XmlNode xn in bodyParams)
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    short itemCatId = short.Parse(xe.GetAttribute("itemCatId"));
                                    short itemId = short.Parse(xe.GetAttribute("itemId"));
                                    if (itemCatId == 1)
                                    {
                                        if (!(KartExcData.character.Contains(itemId)))
                                        {
                                            KartExcData.character.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 2)
                                    {
                                        if (!(KartExcData.color.Contains(itemId)))
                                        {
                                            KartExcData.color.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 3)
                                    {
                                        if (!(KartExcData.kart.Contains(itemId)))
                                        {
                                            KartExcData.kart.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 4)
                                    {
                                        if (!(KartExcData.plate.Contains(itemId)))
                                        {
                                            KartExcData.plate.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 7)
                                    {
                                        if (!(KartExcData.slotChanger.Contains(itemId)))
                                        {
                                            KartExcData.slotChanger.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 8)
                                    {
                                        if (!(KartExcData.goggle.Contains(itemId)))
                                        {
                                            KartExcData.goggle.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 9)
                                    {
                                        if (!(KartExcData.balloon.Contains(itemId)))
                                        {
                                            KartExcData.balloon.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 11)
                                    {
                                        if (!(KartExcData.headBand.Contains(itemId)))
                                        {
                                            KartExcData.headBand.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 12)
                                    {
                                        if (!(KartExcData.headPhone.Contains(itemId)))
                                        {
                                            KartExcData.headPhone.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 13)
                                    {
                                        if (!(KartExcData.ticket.Contains(itemId)))
                                        {
                                            KartExcData.ticket.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 14)
                                    {
                                        if (!(KartExcData.upgradeKit.Contains(itemId)))
                                        {
                                            KartExcData.upgradeKit.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 16)
                                    {
                                        if (!(KartExcData.handGearL.Contains(itemId)))
                                        {
                                            KartExcData.handGearL.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 18)
                                    {
                                        if (!(KartExcData.uniform.Contains(itemId)))
                                        {
                                            KartExcData.uniform.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 20)
                                    {
                                        if (!(KartExcData.decal.Contains(itemId)))
                                        {
                                            KartExcData.decal.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 21)
                                    {
                                        if (!(KartExcData.pet.Contains(itemId)))
                                        {
                                            KartExcData.pet.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 22)
                                    {
                                        if (!(KartExcData.initialCard.Contains(itemId)))
                                        {
                                            KartExcData.initialCard.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 23)
                                    {
                                        if (!(KartExcData.card.Contains(itemId)))
                                        {
                                            KartExcData.card.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 26)
                                    {
                                        if (!(KartExcData.aura.Contains(itemId)))
                                        {
                                            KartExcData.aura.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 27)
                                    {
                                        if (!(KartExcData.skidMark.Contains(itemId)))
                                        {
                                            KartExcData.skidMark.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 28)
                                    {
                                        if (!(KartExcData.roomCard.Contains(itemId)))
                                        {
                                            KartExcData.roomCard.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 31)
                                    {
                                        if (!(KartExcData.ridColor.Contains(itemId)))
                                        {
                                            KartExcData.ridColor.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 32)
                                    {
                                        if (!(KartExcData.rpLucciBonus.Contains(itemId)))
                                        {
                                            KartExcData.rpLucciBonus.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 37)
                                    {
                                        if (!(KartExcData.socket.Contains(itemId)))
                                        {
                                            KartExcData.socket.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 38)
                                    {
                                        if (!(KartExcData.tune.Contains(itemId)))
                                        {
                                            KartExcData.tune.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 39)
                                    {
                                        if (!(KartExcData.resetSocket.Contains(itemId)))
                                        {
                                            KartExcData.resetSocket.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 43)
                                    {
                                        if (!(KartExcData.tuneEnginePatch.Contains(itemId)))
                                        {
                                            KartExcData.tuneEnginePatch.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 44)
                                    {
                                        if (!(KartExcData.tuneHandle.Contains(itemId)))
                                        {
                                            KartExcData.tuneHandle.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 45)
                                    {
                                        if (!(KartExcData.tuneWheel.Contains(itemId)))
                                        {
                                            KartExcData.tuneWheel.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 46)
                                    {
                                        if (!(KartExcData.tuneSupportKit.Contains(itemId)))
                                        {
                                            KartExcData.tuneSupportKit.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 49)
                                    {
                                        if (!(KartExcData.enchantProtect.Contains(itemId)))
                                        {
                                            KartExcData.enchantProtect.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 52)
                                    {
                                        if (!(KartExcData.flyingPet.Contains(itemId)))
                                        {
                                            KartExcData.flyingPet.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 53)
                                    {
                                        if (!(KartExcData.enchantProtect2.Contains(itemId)))
                                        {
                                            KartExcData.enchantProtect2.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 61)
                                    {
                                        if (!(KartExcData.tachometer.Contains(itemId)))
                                        {
                                            KartExcData.tachometer.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 67)
                                    {
                                        if (!(KartExcData.partsPiece.Contains(itemId)))
                                        {
                                            KartExcData.partsPiece.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 68)
                                    {
                                        if (!(KartExcData.partsCoating.Contains(itemId)))
                                        {
                                            KartExcData.partsCoating.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 69)
                                    {
                                        if (!(KartExcData.partsTailLamp.Contains(itemId)))
                                        {
                                            KartExcData.partsTailLamp.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 70)
                                    {
                                        if (!(KartExcData.dye.Contains(itemId)))
                                        {
                                            KartExcData.dye.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 71)
                                    {
                                        if (!(KartExcData.slotBg.Contains(itemId)))
                                        {
                                            KartExcData.slotBg.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 72)
                                    {
                                        if (!(KartExcData.partsEngine12.Contains(itemId)))
                                        {
                                            KartExcData.partsEngine12.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 73)
                                    {
                                        if (!(KartExcData.partsHandle12.Contains(itemId)))
                                        {
                                            KartExcData.partsHandle12.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 74)
                                    {
                                        if (!(KartExcData.partsWheel12.Contains(itemId)))
                                        {
                                            KartExcData.partsWheel12.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 75)
                                    {
                                        if (!(KartExcData.partsBooster12.Contains(itemId)))
                                        {
                                            KartExcData.partsBooster12.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 76)
                                    {
                                        if (!(KartExcData.partsCoating12.Contains(itemId)))
                                        {
                                            KartExcData.partsCoating12.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 77)
                                    {
                                        if (!(KartExcData.partsTailLamp12.Contains(itemId)))
                                        {
                                            KartExcData.partsTailLamp12.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 78)
                                    {
                                        if (!(KartExcData.partsBoosterEffect12.Contains(itemId)))
                                        {
                                            KartExcData.partsBoosterEffect12.Add(itemId);
                                        }
                                    }
                                    else if (itemCatId == 79)
                                    {
                                        if (!(KartExcData.ethisItem.Contains(itemId)))
                                        {
                                            KartExcData.ethisItem.Add(itemId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (PackFolderInfo packFolderInfo2 in packFolderInfo1.GetFoldersInfo())
                    packFolderInfoQueue.Enqueue(packFolderInfo2);
            }
        }

        static byte[] ReplaceBytes(byte[] data)
        {
            byte[] oldBytes = new byte[] {
            0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C, 0x00, 0x20, 0x00,
            0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00,
            0x3D, 0x00, 0x27, 0x00, 0x31, 0x00, 0x2E, 0x00, 0x30, 0x00, 0x27, 0x00, 0x20, 0x00,
            0x65, 0x00, 0x6E, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x64, 0x00, 0x69, 0x00, 0x6E, 0x00,
            0x67, 0x00, 0x3D, 0x00, 0x27, 0x00, 0x55, 0x00, 0x54, 0x00, 0x46, 0x00, 0x2D, 0x00,
            0x31, 0x00, 0x36, 0x00, 0x27, 0x00, 0x3F, 0x00, 0x3E, 0x00, 0x0D, 0x00, 0x0A, 0x00,
            0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C, 0x00, 0x20, 0x00,
            0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00,
            0x3D, 0x00, 0x27, 0x00, 0x31, 0x00, 0x2E, 0x00, 0x30, 0x00, 0x27, 0x00, 0x20, 0x00,
            0x65, 0x00, 0x6E, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x64, 0x00, 0x69, 0x00, 0x6E, 0x00,
            0x67, 0x00, 0x3D, 0x00, 0x27, 0x00, 0x55, 0x00, 0x54, 0x00, 0x46, 0x00, 0x2D, 0x00,
            0x31, 0x00, 0x36, 0x00, 0x27, 0x00, 0x3F, 0x00, 0x3E, 0x00, 0x0D, 0x00, 0x0A, 0x00
        };

            byte[] newBytes = new byte[] {
            0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C, 0x00, 0x20, 0x00,
            0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00,
            0x3D, 0x00, 0x27, 0x00, 0x31, 0x00, 0x2E, 0x00, 0x30, 0x00, 0x27, 0x00, 0x20, 0x00,
            0x65, 0x00, 0x6E, 0x00, 0x63, 0x00, 0x6F, 0x00, 0x64, 0x00, 0x69, 0x00, 0x6E, 0x00,
            0x67, 0x00, 0x3D, 0x00, 0x27, 0x00, 0x55, 0x00, 0x54, 0x00, 0x46, 0x00, 0x2D, 0x00,
            0x31, 0x00, 0x36, 0x00, 0x27, 0x00, 0x3F, 0x00, 0x3E, 0x00, 0x0D, 0x00, 0x0A, 0x00
        };
            int oldLength = oldBytes.Length;
            int newLength = newBytes.Length;
            int dataLength = data.Length;

            byte[] result = new byte[dataLength];
            int resultIndex = 0;
            int i = 0;

            while (i < dataLength)
            {
                bool found = true;
                for (int j = 0; j < oldLength; j++)
                {
                    if (i + j >= dataLength || data[i + j] != oldBytes[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    for (int k = 0; k < newLength; k++)
                    {
                        result[resultIndex++] = newBytes[k];
                    }
                    i += oldLength;
                }
                else
                {
                    result[resultIndex++] = data[i++];
                }
            }

            Array.Resize(ref result, resultIndex);
            return result;
        }

        private static string ReplacePath(string file)
        {
            return file.IndexOf(".rho") > -1 ? file.Substring(0, file.IndexOf(".rho")).Replace("_", "/") + file.Substring(file.IndexOf(".rho") + 4) : file;
        }
    }
}
