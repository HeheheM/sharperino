﻿using System.Net;

#region

using System;
using LeagueSharp;
using System.Linq;
using LeagueSharp.Common;

#endregion

// Server Potition Fixed
namespace D_Graves
{
    class Program
    {
        private const string ChampionName = "Graves";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static Int32 _lastSkin;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 950F);
            _w = new Spell(SpellSlot.W, 950f);
            _e = new Spell(SpellSlot.E, 425f);
            _r = new Spell(SpellSlot.R, 1200f);

            _q.SetSkillshot(0.26f, 5f*2*(float) Math.PI/180, 1950, false, SkillshotType.SkillshotCone);
            _w.SetSkillshot(0.30f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            _r.SetSkillshot(0.22f, 150f, 2100, true, SkillshotType.SkillshotLine);


            //D Graves
            _config = new Menu("D-Graves", "D-Graves", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("UseECR", "Use E targ. Dist.>").SetValue(new Slider(700, 450, 1200)));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRrush", "Rush R if ComboDmg>=Tagret HP")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("autoattack", "Autoattack to calc. the ComboDmg").SetValue(new Slider(3, 1, 6)));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "R if R.DMG>Targ. HP")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRE", "Auto R if targ>=")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("MinTargets", "Ult when>=min enemy(COMBO)").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W")).SetValue(true);
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Press)));

            //LaneClear
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddItem(new MenuItem("UseQL", "Q LaneClear")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseWL", "W LaneClear")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseQLH", "Q LastHit")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseWLH", "W LastHit")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseQJ", "Q Jungle")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseWJ", "W Jungle")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("ActiveLast", "LastHit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("ActiveLane", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQM", "Use Q KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseWM", "Use W KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRM", "Use R KillSteal")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Gap_E", "GapClosers W")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("skinG", "Use Custom Skin").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("skinGraves", "Skin Changer").SetValue(new Slider(4, 1, 6)));
            _config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Usepackes")).SetValue(true);

            //HitChance
            _config.AddSubMenu(new Menu("HitChance", "HitChance"));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("HitChance")
                .SubMenu("Combo")
                .AddItem(
                    new MenuItem("Qchange", "Q Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance")
                .SubMenu("Combo")
                .AddItem(
                    new MenuItem("Wchange", "W Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance")
                .SubMenu("Combo")
                .AddItem(
                    new MenuItem("Rchange", "R Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("HitChance")
                .SubMenu("Harass")
                .AddItem(
                    new MenuItem("Qchangeharass", "Q Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance")
                .SubMenu("Harass")
                .AddItem(
                    new MenuItem("Wchangeharass", "W Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("KillSteal", "KillSteal"));
            _config.SubMenu("HitChance")
                .SubMenu("KillSteal")
                .AddItem(
                    new MenuItem("Qchangekill", "Q Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance")
                .SubMenu("KillSteal")
                .AddItem(
                    new MenuItem("Wchangekill", "W Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance")
                .SubMenu("KillSteal")
                .AddItem(
                    new MenuItem("Rchangekill", "R Hit").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();
            Game.PrintChat("<font color='#881df2'>D-Graves by Diabaths</font> Loaded.");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            if (_config.Item("skinG").GetValue<bool>())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinGraves").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinGraves").GetValue<Slider>().Value;
            }
            //credits to eXit_ / ikkeflikkeri
            WebClient wc = new WebClient();
            wc.Proxy = null;

            wc.DownloadString("http://league.square7.ch/put.php?name=D-" + ChampionName);
            // +1 in Counter (Every Start / Reload) 
            string amount = wc.DownloadString("http://league.square7.ch/get.php?name=D-" + ChampionName);
            // Get the Counter Data
            int intamount = Convert.ToInt32(amount); // remove unneeded line from webhost
            Game.PrintChat("<font color='#881df2'>D-" + ChampionName + "</font> has been started <font color='#881df2'>" +
                           intamount + "</font> Times.");// Post Counter Data
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_config.Item("skinG").GetValue<bool>() && SkinChanged())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinGraves").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinGraves").GetValue<Slider>().Value;
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active || _config.Item("harasstoggle").GetValue<KeyBind>().Active) && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();

            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                Laneclear();
                JungleClear();

            }
            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                LastHit();
            }

            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            KillSteal();
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range) && _config.Item("Gap_E").GetValue<bool>())
            {
                _w.Cast(gapcloser.Sender, Packets());
            }
        }
        static void GenModelPacket(string champ, int skinId)
        {
            Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(_player.NetworkId, skinId, champ)).Process();
        }

        static bool SkinChanged()
        {
            return (_config.Item("skinGraves").GetValue<Slider>().Value != _lastSkin);
        }
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q) * 1.3;
            if (_w.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.W);
            if (_r.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.R);
            damage += _player.GetAutoAttackDamage(enemy, true) * _config.Item("autoattack").GetValue<Slider>().Value;
            return (float)damage;
        }
        private static void Combo()
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var useE = _config.Item("UseEC").GetValue<bool>();
            var useR = _config.Item("UseRC").GetValue<bool>();
            var rangeE = _config.Item("UseECR").GetValue<Slider>().Value;
            var autoR = _config.Item("UseRE").GetValue<bool>();
            var rushUlti = _config.Item("UseRrush").GetValue<bool>();
            if (useQ && _q.IsReady())
            {
                var t = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) < _q.Range - 30 && _q.GetPrediction(t).Hitchance >= Qchange())
                    _q.Cast(t, Packets(), true);
            }
            if (useW && _w.IsReady())
            {
                var t = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) < _w.Range && _w.GetPrediction(t).Hitchance >= Wchange())
                    _w.Cast(t, Packets(), true);
            }
            if (useE && _e.IsReady())
            {
                var t = SimpleTs.GetTarget(_q.Range + 400, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) >= rangeE)
                    _e.Cast(Game.CursorPos);
                return;
            }
            if (_r.IsReady())
            {
                var t = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);
                if (t != null && !t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                    _r.GetPrediction(t).Hitchance >= Rchange())
                {
                    if (ComboDamage(t) > t.Health && rushUlti)
                    {
                        _r.Cast(t, Packets(), true);
                    }
                    if (_r.GetDamage(t) > t.Health && useR)
                    {
                        _r.Cast(t, Packets(), true);
                    }
                }
            }

            if (_r.IsReady() && autoR)
            {
                var t = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);
                PredictionOutput rPred = _r.GetPrediction(t);
                if (ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(_r.Range)) >=
                    _config.Item("MinTargets").GetValue<Slider>().Value
                    && _r.GetPrediction(t).Hitchance >= Rchange())
                    _r.Cast(t, Packets(), true);
            }
        }
        
        private static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var combo = _config.Item("ActiveCombo").GetValue<KeyBind>().Active;
            if (combo && unit.IsMe && (target is Obj_AI_Hero))
            {
                if (useQ && _q.IsReady())
                {
                    var t = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
                    if (t != null && _player.Distance(t) < _q.Range - 30 && _q.GetPrediction(t).Hitchance >= Qchange())
                        _q.Cast(t, Packets(), true);
                }
                if (useW && _w.IsReady())
                {
                    var t = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
                    if (t != null && _player.Distance(t) < _w.Range && _w.GetPrediction(t).Hitchance >= Wchange())
                        _w.Cast(t, Packets(), true);
                }
            }
        }

        private static void Harass()
        {
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useW = _config.Item("UseWH").GetValue<bool>();

            if (useQ && _q.IsReady())
            {
                var t = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) < _q.Range - 30 && _q.GetPrediction(t).Hitchance >= Qchangeharass())
                    _q.Cast(t, Packets(), true);
            }
            if (useW && _w.IsReady())
            {
                var t = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) < _w.Range && _w.GetPrediction(t).Hitchance >= Wchangeharass())
                    _w.Cast(t, Packets(), true);
            }
        }

        private static void Laneclear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
                MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
                MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All);
            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range + _w.Width + 50,
                MinionTypes.Ranged);
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useWl = _config.Item("UseWL").GetValue<bool>();
            if (_q.IsReady() && useQl)
            {
                var fl2 = _q.GetLineFarmLocation(allMinionsQ, _q.Width);

                if (fl2.MinionsHit >= 3)
                {
                    _q.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }

            if (_w.IsReady() && useWl)
            {
                var fl1 = _w.GetCircularFarmLocation(rangedMinionsW, _w.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _w.Cast(fl1.Position);
                }
                else
                    foreach (var minion in allMinionsW)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.W))
                            _w.Cast(minion);
            }
        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useW = _config.Item("UseWLH").GetValue<bool>();
            if (allMinions.Count < 3) return;
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }

                if (_w.IsReady() && useW && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.W))
                {
                    _w.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
                       MinionTypes.All,
                       MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useW = _config.Item("UseWJ").GetValue<bool>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && _q.IsReady())
                {
                    _q.Cast(mob, Packets());
                }
                if (_w.IsReady() && useW)
                {
                    _w.Cast(mob, Packets());
                }
            }
        }

        private static HitChance Qchange()
        {
            switch (_config.Item("Qchange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Wchange()
        {
            switch (_config.Item("Wchange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Rchange()
        {
            switch (_config.Item("Rchange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }
        private static HitChance Qchangeharass()
        {
            switch (_config.Item("Qchangeharass").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Wchangeharass()
        {
            switch (_config.Item("Wchangeharass").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }
        private static HitChance Qchangekill()
        {
            switch (_config.Item("Qchangekill").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Wchangekill()
        {
            switch (_config.Item("Wchangekill").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Rchangekill()
        {
            switch (_config.Item("Rchangekill").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }

        private static void KillSteal()
        {
            if (_q.IsReady() && _config.Item("UseQM").GetValue<bool>())
            {
                var t = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
                if (_q.GetDamage(t) > t.Health && _player.Distance(t) <= _q.Range - 30 && _q.GetPrediction(t).Hitchance >= Qchangekill())
                {
                    _q.Cast(t, Packets(), true);
                }
            }
            if (_w.IsReady() && _config.Item("UseWM").GetValue<bool>())
            {
                var t = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
                if (_w.GetDamage(t) > t.Health && _player.Distance(t) <= _w.Range && _q.GetPrediction(t).Hitchance >= Wchangekill())
                {
                    _w.Cast(t, Packets(), true);
                }
            }
            if (_r.IsReady() && _config.Item("UseRM").GetValue<bool>())
            {
                var t = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);
                if (t != null)
                    if (!t.HasBuff("JudicatorIntervention") && !t.HasBuff("Undying Rage") &&
                        _r.GetDamage(t) > t.Health && _r.GetPrediction(t).Hitchance >= Rchangekill())
                        _r.Cast(t, Packets(), true);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}