using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace DJRyze
{
    class Program
    {
        // Fields and Functions

        private static Orbwalking.Orbwalker Orbwalker;
        private static Menu Config;
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
            if (ObjectManager.Player.ChampionName != "Ryze")
            {
                return;
            }

            Player = ObjectManager.Player;

            Notifications.AddNotification("DJ Ryze loaded", 10000);

            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            Ignite = Player.GetSpellSlot("summonerdot");

            // Menu

            Config = new Menu("DJ Ryze", "Ryze", true);

            var tsMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("useqcombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("usewcombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("useecombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("usercombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("combohm", "Health Manager").SetValue(new Slider(50, 1, 100)));
            Config.SubMenu("Combo").AddItem(new MenuItem("useignitecombo", "Use Ignite").SetValue(true));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("useqharass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("usewharass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("useeharass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("userharass", "Use R").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harasshm", "Health Manager").SetValue(new Slider(50, 1, 100)));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassmm", "Mana Manager").SetValue(new Slider(50, 1, 100)));

            Config.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("useqlane", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("usewlane", "Use W").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("useelane", "Use E").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("userlane", "Use R").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("lanehm", "Health Manager").SetValue(new Slider(50, 1, 100)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("lanemm", "Mana Manager").SetValue(new Slider(50, 1, 100)));

            Config.AddSubMenu(new Menu("Last Hit", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("useqlast", "Use Q").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("usewlast", "Use W").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("useelast", "Use E").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("lastmm", "Mana Manager").SetValue(new Slider(50, 1, 100)));

            Config.SubMenu("LastHit").AddItem(new MenuItem("lasthitkeybinding", "Last Hit Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("LastHit").AddItem(new MenuItem("lasthitqtoggle", "Q Last Hit (Toggle)").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("Jungle Clear", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("useqjungle", "Use Q").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("usewjungle", "Use W").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("useejungle", "Use E").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("userjungle", "Use R").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("junglehm", "Health Manager").SetValue(new Slider(50, 1, 100)));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("junglemm", "Mana Manager").SetValue(new Slider(50, 1, 100)));

            Config.AddSubMenu(new Menu("Kill Steal", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("killq", "Use Q").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("killw", "Use W").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("kille", "Use E").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("killignite", "Use Ignite").SetValue(true));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("miscignite", "Use Ignite").SetValue(new StringList(new[] { "Combo", "Kill Steal" })));
            Config.SubMenu("Misc").AddItem(new MenuItem("rfirst", "Use R first").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("wgapcloser", "Use W on GapCloser").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawq", "Draw Q").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("draww", "Draw W").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawe", "Draw E").SetValue(true));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += Enemy_GapCloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        // Methods
        static void Enemy_GapCloser(ActiveGapcloser args)
        {
            if (Config.Item("wgapcloser").GetValue<bool>())
            {
                if (args.Sender.IsValidTarget(W.Range) && Player.Distance(args.End) < Player.Distance(args.Start))
                {
                    W.Cast(args.Sender);
                }
            }
        }
        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("drawq").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Green);
            }
            if (Config.Item("draww").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Red);
            }
            if (Config.Item("drawe").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Blue);
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
        static void Combo(Obj_AI_Hero Target)
        {
            if (!Target.IsValidTarget())
            {
                return;
            }
            if (Config.Item("rfirst").GetValue<bool>() && (Config.Item("usercombo").GetValue<bool>() && R.IsReady()))
            {
                R.Cast();
            }
            if (Config.Item("useqcombo").GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnUnit(Target);
            }
            if (!Q.IsReady())
            {
                if (Config.Item("usewcombo").GetValue<bool>() && W.IsReady())
                {
                    W.CastOnUnit(Target);
                }
                if (Config.Item("useecombo").GetValue<bool>() && E.IsReady())
                {
                    E.CastOnUnit(Target);
                }
                if (Player.HealthPercentage() <= Config.Item("combohm").GetValue<Slider>().Value)
                {
                    if (Config.Item("usercombo").GetValue<bool>() && R.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
            if (Player.Distance(Target.Position) <= 600 && IgniteDamage(Target) >= Target.Health && Config.Item("useignitecombo").GetValue<bool>() && Config.Item("miscignite").GetValue<StringList>().SelectedIndex == 0)
            {
                Player.Spellbook.CastSpell(Ignite, Target);
            }
        }
        static void Harass(Obj_AI_Hero Target)
        {
            if (!Target.IsValidTarget())
            {
                return;
            }
            if (Player.ManaPercentage() >= Config.Item("harassmm").GetValue<Slider>().Value)
            {
                if (Config.Item("rfirst").GetValue<bool>() && (Config.Item("userharass").GetValue<bool>() && R.IsReady()))
                {
                    R.Cast();
                }
                if (Config.Item("useqharass").GetValue<bool>() && Q.IsReady())
                {
                    Q.CastOnUnit(Target);
                }
                if (!Q.IsReady())
                {
                    if (Config.Item("useeharass").GetValue<bool>() && W.IsReady())
                    {
                        W.CastOnUnit(Target);
                    }
                    if (Config.Item("usewharass").GetValue<bool>() && E.IsReady())
                    {
                        E.CastOnUnit(Target);
                    }
                    if (Player.HealthPercentage() <= Config.Item("harasshm").GetValue<Slider>().Value)
                    {
                        if (Config.Item("userharass").GetValue<bool>() && R.IsReady())
                        {
                            R.Cast();
                        }
                    }
                }
            }
        }
        static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Player.ManaPercentage() >= Config.Item("lanemm").GetValue<Slider>().Value)
            {
                if (Config.Item("useqlane").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            Q.CastOnUnit(minion);
                        }
                    }
                }
                if (Config.Item("usewlane").GetValue<bool>() && W.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            W.CastOnUnit(minion);
                        }
                    }
                }
                if (Config.Item("useelane").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            E.CastOnUnit(minion);
                        }
                    }
                }
                if (Player.HealthPercentage() <= Config.Item("lanehm").GetValue<Slider>().Value)
                {
                    if (Config.Item("userlane").GetValue<bool>() && R.IsReady())
                    {
                        foreach (var minion in allMinions)
                        {
                            if (minion.IsValidTarget())
                            {
                                R.Cast();
                            }
                        }
                    }
                }
            }
        }
        static void JungleClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (Player.ManaPercentage() >= Config.Item("junglemm").GetValue<Slider>().Value)
            {
                if (Config.Item("useqjungle").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            Q.CastOnUnit(minion);
                        }
                    }
                }
                if (Config.Item("usewjungle").GetValue<bool>() && W.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            W.CastOnUnit(minion);
                        }
                    }
                }
                if (Config.Item("useejungle").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            E.CastOnUnit(minion);
                        }
                    }
                }
                if (Player.HealthPercentage() <= Config.Item("junglehm").GetValue<Slider>().Value)
                {
                    if (Config.Item("userjungle").GetValue<bool>() && R.IsReady())
                    {
                        foreach (var minion in allMinions)
                        {
                            if (minion.IsValidTarget())
                            {
                                R.Cast();
                            }
                        }
                    }
                }
            }
        }
        static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Player.ManaPercentage() >= Config.Item("lastmm").GetValue<Slider>().Value)
            {
                if (Config.Item("lasthitqtoggle").GetValue<KeyBind>().Active && !Player.IsRecalling())
                {
                    {
                        foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                        {
                            if (minion.IsValidTarget())
                            {
                                Q.CastOnUnit(minion);
                                return;
                            }
                        }
                    }
                }
                var keyActive = Config.Item("lasthitkeybinding").GetValue<KeyBind>().Active;
                if (!keyActive)
                    return;
                if (Config.Item("useqlast").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (minion.IsValidTarget())
                        {
                            Q.CastOnUnit(minion);
                        }
                    }
                }
                if (Config.Item("usewlast").GetValue<bool>() && W.IsReady())
                {
                    foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W)))
                    {
                        if (minion.IsValidTarget())
                        {
                            W.CastOnUnit(minion);
                        }
                    }
                }
                if (Config.Item("useelast").GetValue<bool>() && E.IsReady())
                {
                    foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W)))
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
            if (Config.Item("killq").GetValue<bool>() && Q.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.Q)))
                {
                    if (champ.IsValidTarget())
                    {
                        Q.CastOnUnit(champ);
                    }
                }
            }
            if (Config.Item("killw").GetValue<bool>() && W.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.W)))
                {
                    if (champ.IsValidTarget())
                    {
                        W.CastOnUnit(champ);
                    }
                }
            }
            if (Config.Item("kille").GetValue<bool>() && E.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.E)))
                {
                    if (champ.IsValidTarget())
                    {
                        E.CastOnUnit(champ);
                    }
                }
            }
            if (Player.Distance(Target.Position) <= 600 && IgniteDamage(Target) >= Target.Health && Config.Item("useignitecombo").GetValue<bool>() && Config.Item("killignite").GetValue<bool>() && Config.Item("miscignite").GetValue<StringList>().SelectedIndex == 1)
            {
                Player.Spellbook.CastSpell(Ignite, Target);
            }
        }
    }
}