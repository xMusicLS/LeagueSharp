using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using Color = System.Drawing.Color;


namespace DJNunu
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
            if (ObjectManager.Player.ChampionName != "Nunu")
            {
                return;
            }
            Player = ObjectManager.Player;
            Notifications.AddNotification("DJ Nunu loaded", 10000);
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 650);
            Ignite = Player.GetSpellSlot("summonerdot");

            // Menu
            Menu = new Menu("DJ Nunu", "Nunu", true);
            var tsMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);
            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("wc", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ec", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("rc", "Use R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("enemiesr", "Enemies for R?").SetValue(new Slider(3, 1, 5)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ignitec", "Use Ignite").SetValue(true));

            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("wh", "Use W").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("eh", "Use E").SetValue(true));

            Menu.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("qlc", "Use Q").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("lanehm", "Health Manager").SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("wlc", "Use W").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("elc", "Use E").SetValue(true));

            Menu.AddSubMenu(new Menu("Jungle Clear", "JungleClear"));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("qjc", "Use Q").SetValue(true));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("junglehm", "Health Manager").SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("wjc", "Use W").SetValue(true));
            Menu.SubMenu("JungleClear").AddItem(new MenuItem("ejc", "Use E").SetValue(true));

            Menu.AddSubMenu(new Menu("Last Hit", "LastHit"));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("qlh", "Use Q").SetValue(true));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("elh", "Use E").SetValue(true));
            Menu.SubMenu("LastHit").AddItem(new MenuItem("lasthitkeybinding", "Last Hit Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Kill Steal", "KillSteal"));
            Menu.SubMenu("KillSteal").AddItem(new MenuItem("eks", "Use E").SetValue(true));
            Menu.SubMenu("KillSteal").AddItem(new MenuItem("rks", "Use R").SetValue(true));
            Menu.SubMenu("KillSteal").AddItem(new MenuItem("igniteks", "Use Ignite").SetValue(true));

            Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("miscignite", "Use Ignite").SetValue(new StringList(new[] { "Combo", "Kill Steal" })));

            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("dq", "Draw Q").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("dw", "Draw W").SetValue(true));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("de", "Draw E").SetValue(true));  
            Menu.SubMenu("Drawings").AddItem(new MenuItem("dr", "Draw R").SetValue(true));

            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        // Methods
        static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("dq").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.BlueViolet);
            }
            if (Menu.Item("dw").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.DeepSkyBlue);
            }
            if (Menu.Item("de").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.RoyalBlue);
            }
            if (Menu.Item("dr").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.AliceBlue);
            }
        }
        static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
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
            if (!target.IsValidTarget())
            {
                return;
            }
            if (Menu.Item("wc").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
            }
            if (Menu.Item("ec").GetValue<bool>() && E.IsReady())
            {
                E.CastOnUnit(target);
            }
            var enemyCount = ObjectManager.Get<Obj_AI_Hero>().Count(e => e.IsValidTarget(R.Range));
            if (Menu.Item("rc").GetValue<bool>() && enemyCount >= Menu.Item("enemiesr").GetValue<Slider>().Value && R.IsReady() && R.IsReady())
            {
                R.Cast();
            }
            if (Player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health && Menu.Item("ignitec").GetValue<bool>() && Menu.Item("miscignite").GetValue<StringList>().SelectedIndex == 0)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }
        static void Harass(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget())
            {
                return;
            }
            if (Menu.Item("wh").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
            }
            if (Menu.Item("eh").GetValue<bool>() && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }
        static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Player.HealthPercentage() <= Menu.Item("lanehm").GetValue<Slider>().Value)
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
            }
            if (Menu.Item("wlc").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
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
        static void JungleClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Player.HealthPercentage() <= Menu.Item("junglehm").GetValue<Slider>().Value)
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
            }
            if (Menu.Item("wjc").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
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
        static void LastHit()
        {
            var keyActive = Menu.Item("lasthitkeybinding").GetValue<KeyBind>().Active;
            if (!keyActive)
                return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Menu.Item("qlh").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (minion.IsValidTarget())
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }
            if (Menu.Item("elh").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    if (minion.IsValidTarget())
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
        }
        static void KillSteal(Obj_AI_Hero Target)
        {
            var Champions = ObjectManager.Get<Obj_AI_Hero>();
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
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.E)))
                {
                    if (champ.IsValidTarget())
                    {
                        E.CastOnUnit(champ);
                    }
                }
            }
            if (Player.Distance(Target.Position) <= 600 && IgniteDamage(Target) >= Target.Health && Menu.Item("ignitec").GetValue<bool>() && Menu.Item("igniteks").GetValue<bool>() && Menu.Item("miscignite").GetValue<StringList>().SelectedIndex == 1)
            {
                Player.Spellbook.CastSpell(Ignite, Target);
            }
        }
    }
}   