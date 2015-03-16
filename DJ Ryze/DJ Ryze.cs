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

            Notifications.AddNotification("DJ Ryze by xMusic", 10000);
            
			Q = new Spell(SpellSlot.Q, 625);
			W = new Spell(SpellSlot.W, 600);
			E = new Spell(SpellSlot.E, 600);
			R = new Spell(SpellSlot.R);

            Ignite = Player.GetSpellSlot("summonerdot");

			Config = new Menu("DJ Ryze", "Ryze", true);

			var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);
            
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("Use Q", "Use Q in Combo?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Use W", "Use W in combo?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Use E", "Use E in combo?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Use R", "Use R in combo?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Use Ignite", "Use Ignite in combo?").SetValue(true));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("Use Q", "Use Q in harass?").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Use W", "Use W in harass?").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Use E", "Use E in harass?").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Use R", "Use R in harass?").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Mana Manager", "Harass MM").SetValue(new Slider(50,1,100)));

            Config.AddSubMenu(new Menu("Lane Clear", "Lane Clear"));
            Config.SubMenu("Lane Clear").AddItem(new MenuItem("Use Q", "Use Q in lane clear?").SetValue(true));
            Config.SubMenu("Lane Clear").AddItem(new MenuItem("Use W", "Use W in lane clear?").SetValue(true));
            Config.SubMenu("Lane Clear").AddItem(new MenuItem("Use E", "Use E in lane clear?").SetValue(true));
            Config.SubMenu("Lane Clear").AddItem(new MenuItem("Use R", "Use R in lane clear?").SetValue(true));
            Config.SubMenu("Lane Clear").AddItem(new MenuItem("Mana Manager", "Lane Clear MM").SetValue(new Slider(50, 1, 100)));

            Config.AddSubMenu(new Menu("Last Hit", "Last Hit"));
            Config.SubMenu("Last Hit").AddItem(new MenuItem("Use Q", "Use Q in last hit?").SetValue(true));
            Config.SubMenu("Last Hit").AddItem(new MenuItem("Use W", "Use W in last hit?").SetValue(true));
            Config.SubMenu("Last Hit").AddItem(new MenuItem("Use E", "Use E in last hit?").SetValue(true));
            Config.SubMenu("Last Hit").AddItem(new MenuItem("Mana Manager", "Last Hit MM").SetValue(new Slider(50, 1, 100)));

            Config.SubMenu("Last Hit").AddItem(new MenuItem("Last Hit Key", "Last Hit Key Binding").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Last Hit").AddItem(new MenuItem("Q Last Hit Toggle", "Last Hit Q Toggle").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("Jungle Clear", "Jungle Clear"));
            Config.SubMenu("Jungle Clear").AddItem(new MenuItem("Use Q", "Use Q in jungle clear?").SetValue(true));
            Config.SubMenu("Jungle Clear").AddItem(new MenuItem("Use W", "Use W in jungle clear?").SetValue(true));
            Config.SubMenu("Jungle Clear").AddItem(new MenuItem("Use E", "Use E in jungle clear?").SetValue(true));
            Config.SubMenu("Jungle Clear").AddItem(new MenuItem("Use R", "Use R in jungle clear?").SetValue(true));
            Config.SubMenu("Jungle Clear").AddItem(new MenuItem("Mana Manager", "Jungle Clear MM").SetValue(new Slider(50, 1, 100)));

            Config.AddSubMenu(new Menu("Kill Steal", "Kill Steal"));
            Config.SubMenu("Kill Steal").AddItem(new MenuItem("Use Q", "Use Q for kill steal?").SetValue(true));
            Config.SubMenu("Kill Steal").AddItem(new MenuItem("Use W", "Use W for kill steal?").SetValue(true));
            Config.SubMenu("Kill Steal").AddItem(new MenuItem("Use E", "Use E for kill steal?").SetValue(true));
            Config.SubMenu("Kill Steal").AddItem(new MenuItem("Use Ignite", "Use Ignite for kill steal?").SetValue(true));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("Use Ignite", "Use Ignite in misc?").SetValue(new StringList(new[]{"Combo", "Kill Steal"})));
            Config.SubMenu("Misc").AddItem(new MenuItem("Use R first?", "Use R first?").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("W on Gap Closer", "Use W on Gap Closer?").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Draw Q", "Draw Q?").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Draw W", "Draw W?").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("Draw E", "Draw E?").SetValue(true));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;

            AntiGapcloser.OnEnemyGapcloser += Enemy_GapCloser;

            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Enemy_GapCloser(ActiveGapcloser args)
        {
            if (Config.Item("Use W on Gap Closer?").GetValue<bool>())
            {
                if (args.Sender.IsValidTarget(W.Range) && Player.Distance(args.End) < Player.Distance(args.Start))
                {
                    W.Cast(args.Sender);
                }
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Draw Q").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Green);
            }
            if (Config.Item("Draw W").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Red);
            }
            if (Config.Item("Draw E").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Blue);
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Magical));
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Magical));
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }
            LastHit();

            var ksTarget = ObjectManager.Get<Obj_AI_Hero>().Where(t => t.IsValidTarget()).OrderBy(t => t.Health).FirstOrDefault();
            if (ksTarget != null)
            KillSteal(ksTarget);
        }

        static void Combo(Obj_AI_Hero Target)
        {
            if (Target == null)
            {
                return;
            }

            if (Config.Item("Use R first?").GetValue<bool>() && R.IsReady())
            {
                R.Cast();
            }

            if (Q.IsReady())
            {
                Q.CastOnUnit(Target);
            }

            if (!Q.IsReady())
            {
                if (W.IsReady())
                {
                    W.CastOnUnit(Target);
                }

                if (E.IsReady())
                {
                    E.CastOnUnit(Target);
                }

                if (R.IsReady())
                {
                    R.Cast();
                }
            }

            if (Player.Distance(Target.Position) <= 600 && IgniteDamage(Target) >= Target.Health && Config.Item("Use Ignite").GetValue<bool>() && Config.Item("Use Ignite in misc?").GetValue<StringList>().SelectedIndex == 0)
            {
                Player.Spellbook.CastSpell(Ignite, Target);
            }

        }

        static void Harass(Obj_AI_Hero Target)
        {
            if (Target == null)
            {
                return;
            }

            if (Player.ManaPercentage() < Config.Item("Harass MM").GetValue<Slider>().Value)

            if (Config.Item("Use R first?").GetValue<bool>() && R.IsReady())
            {
                R.Cast();
            }
            if (Q.IsReady())
            {
                Q.CastOnUnit(Target);
            }
            if (!Q.IsReady())
            {
                if (W.IsReady())
                {
                    W.CastOnUnit(Target);
                }
                if (E.IsReady())
                {
                    E.CastOnUnit(Target);
                }
                if (R.IsReady())
                {
                    R.Cast();
                }
            }
        }

        static void LaneClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (Config.Item("Use Q").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }

            if (Player.ManaPercentage() < Config.Item("Lane Clear MM").GetValue<Slider>().Value)

            if (Config.Item("Use W").GetValue<bool>() && W.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        W.CastOnUnit(minion);
                    }
                }
            }

            if (Config.Item("Use E").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }

            if (Config.Item("Use R").GetValue<bool>() && R.IsReady())
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

        static void JungleClear()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Config.Item("Use Q").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }

            if (Config.Item("Use W").GetValue<bool>() && W.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        W.CastOnUnit(minion);
                    }
                }
            }

            if (Config.Item("Use E").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }

            if (Config.Item("Use R").GetValue<bool>() && R.IsReady())
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

        static void LastHit()
        {
            var keyActive = Config.Item("Last Hit Key Binding").GetValue<KeyBind>().Active;
            if (!keyActive)
                return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (Config.Item("Use Q").GetValue<bool>() && Q.IsReady() && Config.Item("Last Hit Q Toggle").GetValue<KeyBind>().Active)
            {
                foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (minion.IsValidTarget())
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }
            if (Config.Item("Use W").GetValue<bool>() && W.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W)))
                {
                    if (minion.IsValidTarget())
                    {
                        W.CastOnUnit(minion);
                    }
                }
            }
            if (Config.Item("Use E").GetValue<bool>() && E.IsReady())
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

        static void KillSteal(Obj_AI_Hero Target)
        {
            var Champions = ObjectManager.Get<Obj_AI_Hero>();
            if (Config.Item("Use Q").GetValue<bool>() && Q.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.Q)))
                { 
                    if (champ.IsValidTarget())
                    {
                        Q.CastOnUnit(champ);
                    }
                }

            }
            if (Config.Item("Use W").GetValue<bool>() && W.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.W)))
                {
                    if (champ.IsValidTarget())
                    {
                        W.CastOnUnit(champ);
                    }
                }
            }
            if (Config.Item("Use E").GetValue<bool>() && E.IsReady())
            {
                foreach (var champ in Champions.Where(champ => champ.Health <= ObjectManager.Player.GetSpellDamage(champ, SpellSlot.E)))
                {
                    if (champ.IsValidTarget())
                    {
                        E.CastOnUnit(champ);
                    }
                }
            }
            if (Player.Distance(Target.Position) <= 600 && IgniteDamage(Target) >= Target.Health && Config.Item("Use Ignite").GetValue<bool>() && Config.Item("Use Ignite in misc?").GetValue<StringList>().SelectedIndex == 1)
            {
                Player.Spellbook.CastSpell(Ignite, Target);
            }
        }
    }
}