﻿using ExcData;
using KartRider.Common.Utilities;
using KartRider.IO.Packet;
using KartRider_PacketName;
using Set_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KartRider
{
    public static class MultyPlayer
    {
        static string RoomName;
        static byte[] RoomUnkBytes;
        static uint ArrivalTicks, EndTicks, SettleTicks;
        static int channeldata2 = 0;
        static uint track = 0;
        public static uint BootTicksPrev, BootTicksNow;
        public static uint StartTicks = 0;
        static uint FinishTime;

        [DllImport("kernel32")]
        extern static UInt32 GetTickCount();

        public static void milTime(int time)
        {
            GameType.min = time / 60000;
            int sec = time - GameType.min * 60000;
            GameType.sec = sec / 1000;
            GameType.mil = time % 1000;
        }

        public static uint GetUpTime()
        {
            var temp = TimeSpan.FromMilliseconds(GetTickCount()).Ticks;
            temp /= 10000;
            return (uint)temp;
        }

        static void Set_settleTrigger()
        {
            var onceTimer = new System.Timers.Timer();
            onceTimer.Interval = 10000;
            onceTimer.Elapsed += new System.Timers.ElapsedEventHandler((s, _event) => settleTrigger(s, _event));
            onceTimer.AutoReset = false;
            onceTimer.Start();
        }
        static void settleTrigger(object sender, System.Timers.ElapsedEventArgs e)
        {
            SettleTicks = EndTicks + 3100;
            using (OutPacket outPacket = new OutPacket("GameNextStagePacket"))
            {
                outPacket.WriteByte(2);
                outPacket.WriteInt();
                outPacket.WriteInt();
                RouterListener.MySession.Client.Send(outPacket);
            }
            using (OutPacket outPacket = new OutPacket("GameResultPacket"))
            {
                outPacket.WriteByte();
                outPacket.WriteInt(1);
                outPacket.WriteInt();
                outPacket.WriteUInt(FinishTime);
                outPacket.WriteByte();
                outPacket.WriteShort(SetRiderItem.Set_Kart);
                outPacket.WriteShort();
                outPacket.WriteShort();
                outPacket.WriteHexString("d0 78");
                outPacket.WriteByte();
                outPacket.WriteInt(SetRider.RP);
                outPacket.WriteInt(0); //Earned RP
                outPacket.WriteInt(25); //Earned Lucci
                outPacket.WriteUInt(SetRider.Lucci);
                outPacket.WriteBytes(new byte[41]);
                outPacket.WriteInt(1);
                outPacket.WriteShort(768);
                outPacket.WriteBytes(new byte[50]);
                outPacket.WriteInt(255);
                outPacket.WriteInt();
                outPacket.WriteHexString("a8 b8 65 40");
                outPacket.WriteBytes(new byte[162]);
                outPacket.WriteHexString("01 77 00 2d 00 66 00 6f");
                outPacket.WriteBytes(new byte[20]);
                outPacket.WriteHexString("20 00 74 00 ff ff ff ff");
                RouterListener.MySession.Client.Send(outPacket);
            }
            using (OutPacket outPacket = new OutPacket("GameControlPacket"))
            {
                outPacket.WriteInt(4);
                outPacket.WriteHexString("fa 10 69 7f 6a ff 7f 00");
                outPacket.WriteByte();
                outPacket.WriteUInt(SettleTicks);
                outPacket.WriteInt(32767);
                outPacket.WriteInt();
                outPacket.WriteInt(1);
                outPacket.WriteBytes(new byte[21]);
                outPacket.WriteInt(1);
                outPacket.WriteBytes(new byte[28]);
                outPacket.WriteInt(8);
                outPacket.WriteBytes(new byte[6]);
                outPacket.WriteByte(10);
                RouterListener.MySession.Client.Send(outPacket);
            }
            Console.WriteLine("GameSlotPacket, Settle. Ticks = {0}", SettleTicks);
        }

        public static void Clientsession(uint hash, InPacket iPacket)
        {
            if (hash == Adler32Helper.GenerateAdler32_ASCII("GameSlotPacket", 0))
            {
                iPacket.ReadInt();
                iPacket.ReadInt();
                iPacket.ReadInt();
                var nextpacketlenth = iPacket.ReadInt();
                var nextpackethash = iPacket.ReadUInt();
                if (nextpackethash == Adler32Helper.GenerateAdler32_ASCII("GopCourse", 0))
                {
                    iPacket.ReadBytes(nextpacketlenth - 4 - 4);
                    ArrivalTicks = iPacket.ReadUInt();
                }
                Console.WriteLine("GameSlotPacket, Arrivaled. Ticks = {0}", ArrivalTicks);
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("GameControlPacket"))
            {
                var state = iPacket.ReadByte();
                //start
                if (state == 0)
                {
                    BootTicksNow = GetUpTime();
                    StartTicks += (StartTicks == 0) ? (BootTicksNow + 15780) : (BootTicksNow - BootTicksPrev);
                    BootTicksPrev = BootTicksNow;
                    using (OutPacket oPacket = new OutPacket("GameAiMasterSlotNoticePacket"))
                    {
                        oPacket.WriteInt();
                        RouterListener.MySession.Client.Send(oPacket);
                    }
                    using (OutPacket oPacket = new OutPacket("GameControlPacket"))
                    {
                        oPacket.WriteInt(1);
                        oPacket.WriteByte();
                        oPacket.WriteUInt(StartTicks);
                        oPacket.WriteBytes(new byte[71]);
                        oPacket.WriteByte(0x0a);
                        RouterListener.MySession.Client.Send(oPacket);
                    }
                    Console.WriteLine("GameControlPacket, Start. Ticks = {0}", StartTicks);
                }
                //finish
                else if (state == 2)
                {
                    iPacket.ReadInt();
                    FinishTime = iPacket.ReadUInt();
                    using (OutPacket oPacket = new OutPacket("GameRaceTimePacket"))
                    {
                        oPacket.WriteInt();
                        oPacket.WriteUInt(FinishTime);
                        RouterListener.MySession.Client.Send(oPacket);
                    }
                    using (OutPacket oPacket = new OutPacket("GameControlPacket"))
                    {
                        EndTicks = ArrivalTicks + 10180;
                        oPacket.WriteByte(3);
                        oPacket.WriteInt();
                        oPacket.WriteUInt(EndTicks);
                        oPacket.WriteBytes(new byte[71]);
                        oPacket.WriteByte(0x85);
                    }
                    Console.Write("GameControlPacket, Finish. Finish Time = {0}", FinishTime);
                    Console.WriteLine(" , End - Start Ticks : {0}", EndTicks - StartTicks - 10180);
                    Set_settleTrigger();
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("ChGetRoomListRequestPacket"))
            {
                using (OutPacket oPacket = new OutPacket("ChGetRoomListReplyPacket"))
                {
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                    RouterListener.MySession.Client.Send(oPacket);
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("PqChannelSwitch", 0))
            {
                //Console.WriteLine("Channel Switch, avaliable = {0}", iPacket.Available);
                //Console.WriteLine(BitConverter.ToString(iPacket.ReadBytes(iPacket.Available)).Replace("-", " "));
                //iPacket.ReadInt();
                //iPacket.ReadBytes(14);
                byte[] DateTime1 = iPacket.ReadBytes(18);
                byte channel = iPacket.ReadByte();
                Console.WriteLine("Channel Switch, channel = {0}", channel);
                int channeldata1 = 0;
                if (channel == 70)
                {
                    channeldata1 = 1;
                    channeldata2 = 4;
                    //StartGameRacing.GameRacing_SpeedType = 4;
                    using (OutPacket oPacket = new OutPacket("PrChannelSwitch"))
                    {
                        oPacket.WriteInt(0);
                        //oPacket.WriteInt(channeldata1);
                        oPacket.WriteInt(4);
                        oPacket.WriteEndPoint(IPAddress.Parse(RouterListener.client.Address.ToString()), 39312);
                        RouterListener.Listener.BeginAcceptSocket(new AsyncCallback(RouterListener.OnAcceptSocket), null);
                        RouterListener.MySession.Client.Send(oPacket);
                    }
                    GameSupport.OnDisconnect();
                }
                else
                {
                    using (OutPacket oPacket = new OutPacket("ChGetCurrentGpReplyPacket"))
                    {
                        oPacket.WriteInt(0);
                        oPacket.WriteInt(0);
                        oPacket.WriteInt(0);
                        oPacket.WriteInt(0);
                        oPacket.WriteInt(0);
                        oPacket.WriteByte(0);
                        RouterListener.MySession.Client.Send(oPacket);
                    }
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("PqChannelMovein", 0))
            {
                using (OutPacket oPacket = new OutPacket("PrChannelMoveIn"))
                {
                    //oPacket.WriteHexString("01 3d a4 3d 49 8f 99 3d a4 3d 49 90 99");
                    oPacket.WriteByte(1);
                    oPacket.WriteEndPoint(IPAddress.Parse(RouterListener.forceConnect), 39311);
                    oPacket.WriteEndPoint(IPAddress.Parse(RouterListener.forceConnect), 39312);
                    RouterListener.MySession.Client.Send(oPacket);
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("PqMissionAttendPacket", 0))
            {
                using (OutPacket oPacket = new OutPacket("PrMissionAttendPacket"))
                {
                    oPacket.WriteInt(3);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(15);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(-1);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(109);
                    RouterListener.MySession.Client.Send(oPacket);
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("ChCreateRoomRequestPacket", 0))
            {
                Console.Write("Avaiable = {0}", iPacket.Available);
                RoomName = iPacket.ReadString();    //room name
                Console.WriteLine(" RoomName = {0}, len = {1}", RoomName, RoomName.Length);
                iPacket.ReadInt();
                var unk1 = iPacket.ReadByte(); //7c
                var Playernum = iPacket.ReadInt();
                iPacket.ReadInt();
                iPacket.ReadInt();
                RoomUnkBytes = iPacket.ReadBytes(32);
                using (OutPacket oPacket = new OutPacket("ChCreateRoomReplyPacket"))
                {
                    //oPacket.WriteInt(0);
                    oPacket.WriteHexString("01 00");
                    oPacket.WriteByte((byte)Playernum);
                    oPacket.WriteByte(unk1);
                    RouterListener.MySession.Client.Send(oPacket);
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("GrFirstRequestPacket"))
            {
                GrSessionDataPacket();
                //Thread.Sleep(10);
                GrSlotDataPacket();
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("GrChangeTrackPacket"))
            {
                track = iPacket.ReadUInt();
                iPacket.ReadInt();
                RoomUnkBytes = iPacket.ReadBytes(32);
                Console.WriteLine("Gr Track Changed : {0}", RandomTrack.GetTrackName(track));
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("GrRequestSetSlotStatePacket"))
            {
                int Data = iPacket.ReadInt();
                GrSlotDataPacket();
                GrSlotStatePacket(Data);
                GrReplySetSlotStatePacket(Data);
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("GrRequestClosePacket"))
            {
                using (OutPacket oPacket = new OutPacket("GrReplyClosePacket"))
                {
                    //oPacket.WriteHexString("ff 76 05 5d 01");
                    oPacket.WriteUInt(SetRider.UserNO);
                    oPacket.WriteByte(1);
                    oPacket.WriteInt(7);
                    oPacket.WriteInt(7);
                    oPacket.WriteInt(0);
                    oPacket.WriteInt(0);
                    RouterListener.MySession.Client.Send(oPacket);
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("GrRequestStartPacket"))
            {
                using (OutPacket oPacket = new OutPacket("GrReplyStartPacket"))
                {
                    oPacket.WriteInt(0);
                    RouterListener.MySession.Client.Send(oPacket);
                }
                using (OutPacket oPacket = new OutPacket("GrCommandStartPacket"))
                {
                    StartGameData.StartTimeAttack_RandomTrackGameType = 0;
                    StartGameData.StartTimeAttack_SpeedType = 7;
                    StartGameData.StartTimeAttack_Track = track;
                    RandomTrack.SetGameType();

                    oPacket.WriteUInt(Adler32Helper.GenerateAdler32(Encoding.ASCII.GetBytes("GrSessionDataPacket")));
                    GrSessionDataPacket(oPacket);

                    oPacket.WriteUInt(Adler32Helper.GenerateAdler32(Encoding.ASCII.GetBytes("GrSlotDataPacket")));
                    GrSlotDataPacket(oPacket);

                    //kart data
                    PrStartTimeAttack(oPacket);

                    // 6x6 EncFloat
                    oPacket.WriteInt(1);
                    oPacket.WriteEncFloat(0.67f);
                    oPacket.WriteEncFloat(2300);
                    oPacket.WriteEncFloat(2930);
                    oPacket.WriteEncFloat(1.494f);
                    oPacket.WriteEncFloat(1000);
                    oPacket.WriteEncFloat(1500);

                    oPacket.WriteUInt(StartGameData.StartTimeAttack_Track); //track name hash
                    oPacket.WriteInt(10000);

                    oPacket.WriteInt();
                    oPacket.WriteUInt(Adler32Helper.GenerateAdler32(Encoding.ASCII.GetBytes("MissionInfo")));
                    oPacket.WriteHexString("00 00 00 00 00 00 00 00 00 00 ff ff ff ff 00 00 00 00 00");
                    oPacket.WriteString("[applied param]\r\ntransAccelFactor='1.8555' driftEscapeForce='4720' steerConstraint='24.95' normalBoosterTime='3860' \r\npartsBoosterLock='1' \r\n\r\n[equipped / default parts param]\r\ntransAccelFactor='1.86' driftEscapeForce='2120' steerConstraint='2.7' normalBoosterTime='860' \r\n\r\n\r\n[gamespeed param]\r\ntransAccelFactor='-0.0045' driftEscapeForce='2600' steerConstraint='22.25' normalBoosterTime='3000' \r\n\r\n\r\n[factory enchant param]\r\n");
                    RouterListener.MySession.Client.Send(oPacket);
                }
                //StartGameRacing.KartSpecLog();
                Console.WriteLine("Track : {0}", RandomTrack.GetTrackName(StartGameData.StartTimeAttack_Track));
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("PcReportStateInGame", 0))
            {
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("ChLeaveRoomRequestPacket"))
            {
                using (OutPacket oPacket = new OutPacket("ChLeaveRoomReplyPacket"))
                {
                    oPacket.WriteByte(1);
                    RouterListener.MySession.Client.Send(oPacket);
                }
                return;
            }
            else if (hash == Adler32Helper.GenerateAdler32_ASCII("GrRequestBasicAiPacket"))
            {
                int unk1 = iPacket.ReadInt();
                Console.WriteLine("GrRequestBasicAiPacket, unk1 = {0}", unk1);
                using (OutPacket oPacket = new OutPacket("GrSlotDataBasicAi"))
                {
                    oPacket.WriteInt(0);
                    oPacket.WriteByte(1);
                    oPacket.WriteInt(unk1);
                    oPacket.WriteHexString("1400150053040000000016000000000000FFFFFFFF01000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
                    RouterListener.MySession.Client.Send(oPacket);
                }
                using (OutPacket oPacket = new OutPacket("GrReplyBasicAiPacket"))
                {
                    oPacket.WriteByte(1);
                    oPacket.WriteHexString("2CFB6605");
                    RouterListener.MySession.Client.Send(oPacket);
                }
            }
        }

        static void GrSlotDataPacket()
        {
            using (OutPacket oPacket = new OutPacket("GrSlotDataPacket"))
            {
                GrSlotDataPacket(oPacket);
                RouterListener.MySession.Client.Send(oPacket);
            }
        }

        static void GrSlotDataPacket(OutPacket outPacket)
        {
            outPacket.WriteUInt(track); //track name hash
            outPacket.WriteInt(0);
            outPacket.WriteBytes(RoomUnkBytes);
            outPacket.WriteInt(0); //RoomMaster
            outPacket.WriteInt(2);
            outPacket.WriteInt(65535); // outPacket.WriteShort(); outPacket.WriteShort(3);
            outPacket.WriteShort(0); // 797
            outPacket.WriteByte(0);
            var unk1 = 0;
            outPacket.WriteInt(unk1);
            for (int i = 0; i < unk1; i++) outPacket.WriteByte();
            for (int i = 0; i < 4; i++) outPacket.WriteInt();

            /* ---- One/First player ---- */
            outPacket.WriteInt(2);//Player Type, 2 = RoomMaster, 3 = AutoReady, 4 = Observer, 5 = ? , 7 = AI
            outPacket.WriteUInt(SetRider.UserNO);

            outPacket.WriteEndPoint(IPAddress.Parse(RouterListener.client.Address.ToString()), (ushort)RouterListener.client.Port);
            //outPacket.WriteEndPoint(IPAddress.Parse(RouterListener.forceConnect), 39311);
            //outPacket.WriteHexString("3a 16 01 31 7d 48");
            outPacket.WriteInt();
            outPacket.WriteShort();

            outPacket.WriteString(SetRider.Nickname);
            outPacket.WriteShort(SetRider.Emblem1);
            outPacket.WriteShort(SetRider.Emblem2);
            outPacket.WriteShort(0);
            outPacket.WriteShort(SetRiderItem.Set_Character);
            outPacket.WriteShort(SetRiderItem.Set_Paint);
            outPacket.WriteShort(SetRiderItem.Set_Kart);
            outPacket.WriteShort(SetRiderItem.Set_Plate);
            outPacket.WriteShort(SetRiderItem.Set_Goggle);
            outPacket.WriteShort(SetRiderItem.Set_Balloon);
            outPacket.WriteShort(0);
            outPacket.WriteShort(SetRiderItem.Set_HeadBand);
            outPacket.WriteShort(0);
            outPacket.WriteShort(SetRiderItem.Set_HandGearL);
            outPacket.WriteShort(0);
            outPacket.WriteShort(SetRiderItem.Set_Uniform);
            outPacket.WriteShort(0); //decal
            outPacket.WriteShort(SetRiderItem.Set_Pet);
            outPacket.WriteShort(SetRiderItem.Set_FlyingPet);
            outPacket.WriteShort(SetRiderItem.Set_Aura);
            outPacket.WriteShort(SetRiderItem.Set_SkidMark);
            outPacket.WriteShort(0);
            outPacket.WriteShort(SetRiderItem.Set_RidColor);
            outPacket.WriteShort(SetRiderItem.Set_BonusCard);
            outPacket.WriteShort(0); //bossModeCard
            var PlantKartAndSN = new { Kart = SetRiderItem.Set_Kart, SN = SetRiderItem.Set_KartSN };
            var plantList = KartExcData.PlantList;
            var existingPlant = plantList.FirstOrDefault(list => list[0] == PlantKartAndSN.Kart && list[1] == PlantKartAndSN.SN);
            if (existingPlant != null)
            {
                outPacket.WriteShort(existingPlant[3]);
                outPacket.WriteShort(existingPlant[7]);
                outPacket.WriteShort(existingPlant[5]);
                outPacket.WriteShort(existingPlant[9]);
            }
            else
            {
                outPacket.WriteShort(0);
                outPacket.WriteShort(0);
                outPacket.WriteShort(0);
                outPacket.WriteShort(0);
            }
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(SetRiderItem.Set_Tachometer);
            outPacket.WriteShort(SetRiderItem.Set_Dye);
            outPacket.WriteShort(SetRiderItem.Set_KartSN);
            outPacket.WriteByte(0);
            var ExcKartAndSN = new { Kart = SetRiderItem.Set_Kart, SN = SetRiderItem.Set_KartSN };
            var partsList = KartExcData.PartsList;
            var existingParts = partsList.FirstOrDefault(list => list[0] == ExcKartAndSN.Kart && list[1] == ExcKartAndSN.SN);
            if (existingParts != null)
            {
                outPacket.WriteShort(existingParts[14]);
                outPacket.WriteShort(existingParts[15]);
            }
            else
            {
                var levelList = KartExcData.LevelList;
                var existingLevel = levelList.FirstOrDefault(list => list[0] == ExcKartAndSN.Kart && list[1] == ExcKartAndSN.SN);
                if (existingLevel != null)
                {
                    outPacket.WriteShort(7);
                    outPacket.WriteShort(0);
                }
                else
                {
                    outPacket.WriteShort(0);
                    outPacket.WriteShort(0);
                }
            }
            outPacket.WriteShort(SetRiderItem.Set_slotBg);
            outPacket.WriteString("");
            outPacket.WriteInt(SetRider.RP);
            outPacket.WriteByte();
            outPacket.WriteByte();
            outPacket.WriteByte();
            for (int i = 0; i < 8; i++) outPacket.WriteInt();
            outPacket.WriteInt(1500); //outPacket.WriteInt(1500);
            outPacket.WriteInt(1499); //outPacket.WriteInt(2000);
            outPacket.WriteInt(0); //outPacket.WriteInt();
            outPacket.WriteInt(2000); //outPacket.WriteInt(2000);
            outPacket.WriteInt(5); //outPacket.WriteInt(5);
            outPacket.WriteByte(255); //outPacket.WriteInt(1677721855);
            outPacket.WriteByte(0);
            outPacket.WriteByte(0);
            outPacket.WriteByte(0);

            outPacket.WriteByte(3); //3
            outPacket.WriteString("");
            outPacket.WriteInt();
            outPacket.WriteInt();
            outPacket.WriteInt();
            outPacket.WriteInt();
            outPacket.WriteByte();
            outPacket.WriteInt();

            for (int i = 0; i < 7; i++) outPacket.WriteInt(0);
            outPacket.WriteBytes(new byte[36]);
            for (int i = 0; i < 7; i++) outPacket.WriteUInt(4294967295); //FFFFFFFF
            outPacket.WriteInt(0);
            /*---- One/First player ----*/

            // AI Data
            /*
            outPacket.WriteInt(7);
            outPacket.WriteShort(1);
            outPacket.WriteShort(0);
            outPacket.WriteShort(1112);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteByte(0);
            outPacket.WriteInt(7);
            outPacket.WriteShort(1);
            outPacket.WriteShort(0);
            outPacket.WriteShort(1111);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteByte(0);
            outPacket.WriteInt(7);
            outPacket.WriteShort(1);
            outPacket.WriteShort(0);
            outPacket.WriteShort(1119);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteByte(0);
            outPacket.WriteInt(7);
            outPacket.WriteShort(1);
            outPacket.WriteShort(0);
            outPacket.WriteShort(1111);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteByte(0);
            outPacket.WriteInt(7);
            outPacket.WriteShort(1);
            outPacket.WriteShort(0);
            outPacket.WriteShort(1108);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteByte(0);
            outPacket.WriteInt(7);
            outPacket.WriteShort(1);
            outPacket.WriteShort(0);
            outPacket.WriteShort(1111);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteByte(0);
            outPacket.WriteInt(7);
            outPacket.WriteShort(1);
            outPacket.WriteShort(0);
            outPacket.WriteShort(1142);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteShort(0);
            outPacket.WriteByte(0);
            outPacket.WriteBytes(new byte[36]);
            outPacket.WriteUInt(4);
            outPacket.WriteUInt(1);
            outPacket.WriteUInt(5);
            outPacket.WriteUInt(2);
            outPacket.WriteUInt(6);
            outPacket.WriteUInt(3);
            outPacket.WriteUInt(7);
            outPacket.WriteInt(0);
            */
        }

        public static void PrStartTimeAttack(OutPacket oPacket)
        {
            float DriftEscapeForce = FlyingPet.DriftEscapeForce + TuneSpec.Tune_DriftEscapeForce + TuneSpec.Plant45_DriftEscapeForce + TuneSpec.KartLevel_DriftEscapeForce + SpeedPatch.DriftEscapeForce;
            float NormalBoosterTime = FlyingPet.NormalBoosterTime + TuneSpec.Tune_NormalBoosterTime + TuneSpec.Plant46_NormalBoosterTime;
            float TransAccelFactor = TuneSpec.Tune_TransAccelFactor + TuneSpec.Plant43_TransAccelFactor + TuneSpec.KartLevel_TransAccelFactor + SpeedPatch.TransAccelFactor;
            //------------------------------------------------------------------------KartSpac Start
            oPacket.WriteEncFloat(Kart.draftMulAccelFactor);
            oPacket.WriteEncInt(Kart.draftTick);
            oPacket.WriteEncFloat(Kart.driftBoostMulAccelFactor);
            oPacket.WriteEncInt(Kart.driftBoostTick);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeed);
            oPacket.WriteEncByte((byte)(Kart.SpeedSlotCapacity + TuneSpec.Plant46_SpeedSlotCapacity));
            oPacket.WriteEncByte((byte)(Kart.ItemSlotCapacity + TuneSpec.Plant46_ItemSlotCapacity));
            oPacket.WriteEncByte(Kart.SpecialSlotCapacity);
            oPacket.WriteEncByte(Kart.UseTransformBooster);
            oPacket.WriteEncByte(Kart.motorcycleType);
            oPacket.WriteEncByte(Kart.BikeRearWheel);
            oPacket.WriteEncFloat(Kart.Mass);
            oPacket.WriteEncFloat(Kart.AirFriction);
            oPacket.WriteEncFloat(SpeedType.DragFactor + Kart.DragFactor + FlyingPet.DragFactor + SpeedPatch.DragFactor + TuneSpec.Tune_DragFactor + TuneSpec.Plant43_DragFactor + TuneSpec.Plant45_DragFactor + TuneSpec.KartLevel_DragFactor);
            oPacket.WriteEncFloat(SpeedType.ForwardAccelForce + Kart.ForwardAccelForce + FlyingPet.ForwardAccelForce + TuneSpec.Tune_ForwardAccel + TuneSpec.Plant43_ForwardAccel + TuneSpec.Plant46_ForwardAccel + TuneSpec.KartLevel_ForwardAccel + SpeedPatch.ForwardAccelForce);
            oPacket.WriteEncFloat(SpeedType.BackwardAccelForce + Kart.BackwardAccelForce);
            oPacket.WriteEncFloat(SpeedType.GripBrakeForce + Kart.GripBrakeForce + TuneSpec.Plant44_GripBrake + TuneSpec.Plant46_GripBrake);
            oPacket.WriteEncFloat(SpeedType.SlipBrakeForce + Kart.SlipBrakeForce + TuneSpec.Plant44_SlipBrake + TuneSpec.Plant45_SlipBrake + TuneSpec.Plant46_SlipBrake);
            oPacket.WriteEncFloat(Kart.MaxSteerAngle);
            if (TuneSpec.PartSpec_SteerConstraint == 0f)
            {
                oPacket.WriteEncFloat(SpeedType.SteerConstraint + Kart.SteerConstraint + TuneSpec.Plant44_SteerConstraint + TuneSpec.KartLevel_SteerConstraint);
            }
            else
            {
                oPacket.WriteEncFloat(TuneSpec.PartSpec_SteerConstraint + SpeedType.AddSpec_SteerConstraint + TuneSpec.Plant44_SteerConstraint + TuneSpec.KartLevel_SteerConstraint);
            }
            oPacket.WriteEncFloat(Kart.FrontGripFactor + TuneSpec.Plant44_FrontGripFactor);
            oPacket.WriteEncFloat(Kart.RearGripFactor + TuneSpec.Plant44_RearGripFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerTime);
            oPacket.WriteEncFloat(Kart.DriftSlipFactor + TuneSpec.Plant46_DriftSlipFactor);
            if (TuneSpec.PartSpec_DriftEscapeForce == 0f)
            {
                oPacket.WriteEncFloat(SpeedType.DriftEscapeForce + Kart.DriftEscapeForce + DriftEscapeForce);
            }
            else
            {
                oPacket.WriteEncFloat(TuneSpec.PartSpec_DriftEscapeForce + SpeedType.AddSpec_DriftEscapeForce + DriftEscapeForce);
            }
            oPacket.WriteEncFloat(SpeedType.CornerDrawFactor + Kart.CornerDrawFactor + FlyingPet.CornerDrawFactor + TuneSpec.Tune_CornerDrawFactor + TuneSpec.Plant44_CornerDrawFactor + TuneSpec.Plant45_CornerDrawFactor + TuneSpec.KartLevel_CornerDrawFactor + SpeedPatch.CornerDrawFactor);
            oPacket.WriteEncFloat(Kart.DriftLeanFactor);
            oPacket.WriteEncFloat(Kart.SteerLeanFactor);
            if (StartGameData.StartTimeAttack_SpeedType == 4 || StartGameData.StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S4_DriftMaxGauge);
            }
            else
            {
                oPacket.WriteEncFloat(SpeedType.DriftMaxGauge + Kart.DriftMaxGauge + TuneSpec.Tune_DriftMaxGauge + TuneSpec.Plant45_DriftMaxGauge + TuneSpec.Plant46_DriftMaxGauge + SpeedPatch.DriftMaxGauge);
            }
            if (StartGameData.StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S6_BoosterTime);
            }
            else
            {
                if (TuneSpec.PartSpec_NormalBoosterTime == 0f)
                {
                    oPacket.WriteEncFloat(Kart.NormalBoosterTime + NormalBoosterTime);
                }
                else
                {
                    oPacket.WriteEncFloat(TuneSpec.PartSpec_NormalBoosterTime + NormalBoosterTime);
                }
            }
            oPacket.WriteEncFloat(Kart.ItemBoosterTime + FlyingPet.ItemBoosterTime);
            if (StartGameData.StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S6_BoosterTime);
            }
            else
            {
                oPacket.WriteEncFloat(Kart.TeamBoosterTime + FlyingPet.TeamBoosterTime + TuneSpec.Tune_TeamBoosterTime + TuneSpec.Plant46_TeamBoosterTime);
            }
            oPacket.WriteEncFloat(Kart.AnimalBoosterTime + TuneSpec.Plant45_AnimalBoosterTime + TuneSpec.Plant46_AnimalBoosterTime);
            oPacket.WriteEncFloat(Kart.SuperBoosterTime);
            if (TuneSpec.PartSpec_TransAccelFactor == 0f)
            {
                oPacket.WriteEncFloat(SpeedType.TransAccelFactor + Kart.TransAccelFactor + TransAccelFactor);
            }
            else
            {
                oPacket.WriteEncFloat(TuneSpec.PartSpec_TransAccelFactor + SpeedType.AddSpec_TransAccelFactor + TransAccelFactor);
            }
            oPacket.WriteEncFloat(SpeedType.BoostAccelFactor + Kart.BoostAccelFactor + SpeedPatch.BoostAccelFactor);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeItem + TuneSpec.KartLevel_StartBoosterTimeItem + TuneSpec.Plant46_StartBoosterTimeItem);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeSpeed + TuneSpec.Tune_StartBoosterTimeSpeed + TuneSpec.Plant43_StartBoosterTimeSpeed + TuneSpec.Plant46_StartBoosterTimeSpeed + TuneSpec.KartLevel_StartBoosterTimeSpeed);
            oPacket.WriteEncFloat(SpeedType.StartForwardAccelForceItem + Kart.StartForwardAccelForceItem + FlyingPet.StartForwardAccelForceItem + SpeedPatch.StartForwardAccelForceItem + TuneSpec.Plant46_StartForwardAccelItem);
            oPacket.WriteEncFloat(SpeedType.StartForwardAccelForceSpeed + Kart.StartForwardAccelForceSpeed + FlyingPet.StartForwardAccelForceSpeed + SpeedPatch.StartForwardAccelForceSpeed + TuneSpec.Plant43_StartForwardAccelSpeed + TuneSpec.Plant46_StartForwardAccelSpeed);
            oPacket.WriteEncFloat(Kart.DriftGaguePreservePercent);
            oPacket.WriteEncByte(Kart.UseExtendedAfterBooster);
            oPacket.WriteEncFloat(Kart.BoostAccelFactorOnlyItem + TuneSpec.KartLevel_BoostAccelFactorOnlyItem);
            oPacket.WriteEncFloat(Kart.antiCollideBalance + TuneSpec.Plant45_AntiCollideBalance);
            oPacket.WriteEncByte(Kart.dualBoosterSetAuto);
            oPacket.WriteEncInt(Kart.dualBoosterTickMin);
            oPacket.WriteEncInt(Kart.dualBoosterTickMax);
            oPacket.WriteEncFloat(Kart.dualMulAccelFactor);
            oPacket.WriteEncFloat(Kart.dualTransLowSpeed);
            oPacket.WriteEncByte(Kart.PartsEngineLock);
            oPacket.WriteEncByte(Kart.PartsWheelLock);
            oPacket.WriteEncByte(Kart.PartsSteeringLock);
            oPacket.WriteEncByte(Kart.PartsBoosterLock);
            oPacket.WriteEncByte(Kart.PartsCoatingLock);
            oPacket.WriteEncByte(Kart.PartsTailLampLock);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoost);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByGrip);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWall);
            oPacket.WriteEncFloat(Kart.instAccelFactor);
            oPacket.WriteEncInt(Kart.instAccelGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.instAccelGaugeLength);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinUsable);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelLoss);
            oPacket.WriteEncByte(Kart.useExtendedAfterBoosterMore);
            oPacket.WriteEncInt(Kart.wallCollGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMaxVelLoss);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelLoss);
            oPacket.WriteEncFloat(Kart.modelMaxX);
            oPacket.WriteEncFloat(Kart.modelMaxY);
            oPacket.WriteEncByte(Kart.defaultExceedType);
            oPacket.WriteEncByte(Kart.v12_1);
            oPacket.WriteEncByte(Kart.v12_2);
            oPacket.WriteEncByte(Kart.v12_3);
            oPacket.WriteEncByte(Kart.defaultEngineType);
            oPacket.WriteEncByte(Kart.v12);
            oPacket.WriteEncByte(Kart.defaultHandleType);
            oPacket.WriteEncByte(Kart.v12);
            oPacket.WriteEncByte(Kart.defaultWheelType);
            oPacket.WriteEncByte(Kart.v12);
            oPacket.WriteEncByte(Kart.defaultBoosterType);
            oPacket.WriteEncByte(Kart.v12);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWallAdded);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoostAdded);
            oPacket.WriteEncByte(Kart.chargerSystemboosterUseCount);
            oPacket.WriteEncByte(Kart.v12_1);
            oPacket.WriteEncByte(Kart.v12_2);
            oPacket.WriteEncByte(Kart.v12_3);
            oPacket.WriteEncFloat(Kart.chargerSystemUseTime);
            oPacket.WriteEncFloat(Kart.v12_4);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeedAdded);
            oPacket.WriteEncFloat(Kart.driftGaugeFactor);
            oPacket.WriteEncFloat(Kart.v12_5);
            oPacket.WriteEncFloat(Kart.v12_6);
            //------------------------------------------------------------------------KartSpac End
        }

        static void GrSlotStatePacket(int Data)
        {
            using (OutPacket oPacket = new OutPacket("GrSlotStatePacket"))
            {
                oPacket.WriteInt(Data);
                oPacket.WriteBytes(new byte[60]);
                RouterListener.MySession.Client.Send(oPacket);
            }
        }

        static void GrReplySetSlotStatePacket(int Data)
        {
            using (OutPacket oPacket = new OutPacket("GrReplySetSlotStatePacket"))
            {
                oPacket.WriteUInt(SetRider.UserNO);
                oPacket.WriteInt(1);
                oPacket.WriteByte(0);
                oPacket.WriteInt(Data);
                RouterListener.MySession.Client.Send(oPacket);
            }
        }

        static void GrSessionDataPacket()
        {
            using (OutPacket oPacket = new OutPacket("GrSessionDataPacket"))
            {
                GrSessionDataPacket(oPacket);
                RouterListener.MySession.Client.Send(oPacket);
            }
        }
        static void GrSessionDataPacket(OutPacket outPacket)
        {
            outPacket.WriteString(RoomName);
            outPacket.WriteInt(0);
            outPacket.WriteByte(1);
            outPacket.WriteByte(7); //(byte)channeldata2
            outPacket.WriteInt(0);
            outPacket.WriteHexString("089637AF70"); //08 24 72 F5 9E
            outPacket.WriteInt(0);
            outPacket.WriteByte(0);
            outPacket.WriteByte(0);
        }
    }
}