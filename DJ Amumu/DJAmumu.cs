using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using Color = System.Drawing.Color;

namespace DJAmumu
{
    class Program
    {
        // Fields and Functions
        private static Orbwalking.Orbwalker Orbwalker;
        private static Menu Menu;
        private static Spell Q, W, E, R;
        private static SpellSlot Ignite;
        private static Obj_AI_Hero Player;
        private static float IgniteDamage(Obj_AI_Base target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Amumu")
            {
                return;
            }
            Player = ObjectManager.Player;
            Notifications.AddNotification("DJ Amumu by xMusic", 10000);
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);
            Q.SetSkillshot(250f, 90f, 2000f, true, SkillshotType.SkillshotLine);
            Ignite = Player.GetSpellSlot("summonerdot");

            // Menu
            Menu = new Menu("DJ Amumu", "Amumu", true);
            var tsMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(tsMenu);

            Menu.AddSubMenu(tsMenu);
            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Use Q", "qc").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Use W", "wc").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Use E", "ec").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Use R", "rc").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Min Enemies for R", "enemiesr").SetValue(new Slider(3, 1, 5)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Use Ignite", "comboignite").SetValue(true));

            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Use Q", "qh").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Use W", "wh").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Use E", "eh").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Mana Manager", "harassmm").SetValue(new Slider(50, 1, 100)));

            Menu.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("Use Q", "qlc").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("Use W", "wlc").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("Use E", "elc").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("Mana Manager", "lanemm").SetValue(new Slider(50, 1, 100)));

            Menu.AddSubMenu(new Menu("Jungle Clear", "JungleClear"));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("Use Q", "qjc").SetValue(true));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("Use W", "wjc").SetValue(true));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("Use E", "ejc").SetValue(true));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("Mana Manager", "junglemm").SetValue(new Slider(50, 1, 100)));

            Menu.AddSubMenu(new Menu("Last Hit", "LastHit"));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("Use Q", "qlh").SetValue(true));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("Use W", "wlh").SetValue(true));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("Use E", "elh").SetValue(true));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("Mana Manager", "lastmm").SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("Last Hit Key", "lasthitkeybinding").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Kill Steal", "KillSteal"));
            Menu.SubMenu("KillSteal").AddItem(new MenuItem("Use Q", "qks").SetValue(true));
            Menu.SubMenu("KillSteal").AddItem(new MenuItem("Use E", "eks").SetValue(true));
            Menu.SubMenu("KillSteal").AddItem(new MenuItem("Use R", "rks").SetValue(true));
            Menu.SubMenu("KillSteal").AddItem(new MenuItem("Use Ignite", "ksignite").SetValue(true));

            Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Ignite Mode", "miscignite").SetValue(new StringList(new[] { "Combo", "Kill Steal" })));

            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw Q", "drawq").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw W", "draww").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw E", "drawe").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw R", "drawr").SetValue(true));

            Menu.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
        }
        // Methods
        static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("drawq").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.ServerPosition, Q.Range, System.Drawing.Color.OrangeRed);
            }
            if (Menu.Item("draww").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.ServerPosition, W.Range, System.Drawing.Color.OrangeRed);
            }
            if (Menu.Item("drawe").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.ServerPosition, E.Range, System.Drawing.Color.OrangeRed);
            }
            if (Menu.Item("drawr").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.ServerPosition, R.Range, System.Drawing.Color.OrangeRed);
            }
        }
        static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(target);
                    break;
            }
            LastHit();
            var ksTarget = ObjectManager.Get<Obj_AI_Hero>().Where(t => t.IsValidTarget()).OrderBy(t => t.Health).FirstOrDefault();
            if (ksTarget != null)
            KillSteal(ksTarget);
        }
        static void Combo(Obj_AI_Hero target)
        {
            if (target == null)
            {
                return;
            }
            if (Menu.Item("qc").GetValue<bool>() && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                Q.CastOnUnit(target);
            }
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                if (Menu.Item("wc").GetValue<bool>() && W.IsReady() && ObjectManager.Get<Obj_AI_Hero>().Any(hero => hero.IsValidTarget(W.Range)))
                {
                    W.Cast();
                }
            }
            if (Menu.Item("ec").GetValue<bool>() && E.IsReady())
            {
                E.Cast(target);
            }
            var enemyCount = ObjectManager.Get<Obj_AI_Hero>().Count(e => e.IsValidTarget(R.Range));
            if (Menu.Item("rc").GetValue<bool>() && enemyCount >= Menu.Item("enemiesr").GetValue<Slider>().Value && R.IsReady())
            {
                R.Cast();
            }
            if (Player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health && Menu.Item("comboignite").GetValue<bool>() && Menu.Item("miscignite").GetValue<StringList>().SelectedIndex == 0)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }
        static void Harass(Obj_AI_Hero target)
        {
            if (target == null)
            {
                return;
            }
            if (Player.ManaPercentage() < Menu.Item("harassmm").GetValue<Slider>().Value)
            {
                if (Menu.Item("qh").GetValue<bool>() && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(target);
                }
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                {
                    if (Menu.Item("wh").GetValue<bool>() && W.IsReady() && ObjectManager.Get<Obj_AI_Hero>().Any(hero => hero.IsValidTarget(W.Range)))
                    {
                        W.Cast();
                    }
                }
                if (Menu.Item("eh").GetValue<bool>() && E.IsReady())
                {
                    E.Cast(target);
                }
            }
        }
        static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Player.ManaPercentage() < Menu.Item("lanemm").GetValue<Slider>().Value)
            {
                if (Menu.Item("qlc").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            Q.CastOnUnit(minion);
                        }
                    }
                }
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                {
                    if (Menu.Item("wlc").GetValue<bool>() && W.IsReady() && ObjectManager.Get<Obj_AI_Minion>().Any(minion => minion.IsValidTarget(W.Range)))
                    {
                        W.Cast();
                    }
                }
                if (Menu.Item("elc").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            E.CastOnUnit(minion);
                        }
                    }
                }
            }
        }
        static void LastHit()
        {
            var keyActive = Menu.Item("lasthitkeybinding").GetValue<KeyBind>().Active;
            if (!keyActive)
                return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Player.ManaPercentage() < Menu.Item("lastmm").GetValue<Slider>().Value)
            {
                if (Menu.Item("qlh").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            Q.CastOnUnit(minion);
                        }
                    }
                }
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                {
                    if (Menu.Item("wlh").GetValue<bool>() && W.IsReady() && ObjectManager.Get<Obj_AI_Minion>().Any(minion => minion.IsValidTarget(W.Range)))
                    {
                        W.Cast();
                    }
                }
                if (Menu.Item("elh").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            E.CastOnUnit(minion);
                        }
                    }
                }
            }
        }
        static void JungleClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Player.ManaPercentage() < Menu.Item("junglemm").GetValue<Slider>().Value)
            {
                if (Menu.Item("qjc").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            Q.CastOnUnit(minion);
                        }
                    }
                }
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                {
                    if (Menu.Item("wjc").GetValue<bool>() && W.IsReady() && ObjectManager.Get<Obj_AI_Minion>().Any(minion => minion.IsValidTarget(W.Range)))
                    {
                        W.Cast();
                    }
                }
                if (Menu.Item("ejc").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            E.CastOnUnit(minion);
                        }
                    }
                }
            }
        }
        static void KillSteal(Obj_AI_Hero Target)
        {
            var Champions = ObjectManager.Get<Obj_AI_Hero>();
            if (Menu.Item("wks").GetValue<bool>() && W.IsReady() && ObjectManager.Get<Obj_AI_Hero>().Any(hero => hero.IsValidTarget(W.Range)))
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.W)))
                {
                    if (champ.IsValidTarget())
                    {
                        W.Cast();
                    }
                }
            }
            if (Menu.Item("eks").GetValue<bool>() && E.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.E)))
                {
                    if (champ.IsValidTarget())
                    {
                        E.CastOnUnit(champ);
                    }
                }
            }
            if (Menu.Item("rks").GetValue<bool>() && E.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.R)))
                {
                    if (champ.IsValidTarget())
                    {
                        R.CastOnUnit(champ);
                    }
                }
            }
            if (Player.Distance(Target.Position) <= 600 && IgniteDamage(Target) >= Target.Health && Menu.Item("comboignite").GetValue<bool>() && Menu.Item("ksignite").GetValue<bool>() && Menu.Item("miscignite").GetValue<StringList>().SelectedIndex == 1)
            {
                Player.Spellbook.CastSpell(Ignite, Target);
            }
        }
    }
}
