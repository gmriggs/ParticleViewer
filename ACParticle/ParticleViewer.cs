using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using ACE.DatLoader;
using ACE.DatLoader.Entity.AnimationHooks;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using ACE.Server.Physics;
using ACE.Server.Physics.Animation;

namespace ACParticle
{
    public class ParticleViewer : WpfGame
    {
        private IGraphicsDeviceService _graphicsDeviceManager;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;

        public KeyboardState PrevKeyboardState;

        public static ParticleViewer Instance;

        public new Render.Render Render;
        public Camera Camera;

        public Player Player;

        public static Particle Window { get => Particle.Instance; }

        protected override void Initialize()
        {
            // must be initialized. required by Content loading and rendering (will add itself to the Services)
            // note that MonoGame requires this to be initialized in the constructor, while WpfInterop requires it to
            // be called inside Initialize (before base.Initialize())
            _graphicsDeviceManager = new WpfGraphicsDeviceService(this);

            // wpf and keyboard need reference to the host control in order to receive input
            // this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);

            Instance = this;

            // must be called after the WpfGraphicsDeviceService instance was created
            base.Initialize();

            // content loading now possible
        }

        public void PostInit()
        {
            InitPlayer();

            Render = new Render.Render();
            Render.Init();
        }

        public void InitPlayer()
        {
            Player = new Player();
        }

        public float GetModIdx(List<float> mods, float mod)
        {
            foreach (var m in mods)
            {
                if (mod >= m)
                    return m;
            }
            return mods[0];
        }

        public List<uint> GetEmitterInfoIDs(uint pEffectTableID, PlayScript playScript, float mod = 0.0f)
        {
            var pEffectTable = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PhysicsScriptTable>(pEffectTableID);
            var scripts = pEffectTable.ScriptTable[(uint)playScript].Scripts;

            var modIdx = GetModIdx(scripts.Select(s => s.Mod).OrderByDescending(m => m).ToList(), mod);

            var scriptModDataEntry = scripts.Where(s => s.Mod == modIdx).FirstOrDefault();

            return GetEmitterInfoIDs(scriptModDataEntry.ScriptId);
        }

        public List<uint> GetEmitterInfoIDs(uint scriptID, float mod = 0.0f)
        {
            var emitterInfoIDs = new List<uint>();

            var script = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PhysicsScript>(scriptID);

            foreach (var scriptDataEntry in script.ScriptData)
            {
                // AnimationHook
                if (scriptDataEntry.Hook.HookType == AnimationHookType.CreateParticle)
                {
                    var particleHook = scriptDataEntry.Hook as CreateParticleHook;
                    var emitterInfoID = particleHook.EmitterInfoId;
                    emitterInfoIDs.Add(emitterInfoID);
                }
            }
            return emitterInfoIDs;
        }

        public void InitEmitter(List<uint> emitterInfoIDs, float mod = 0.0f)
        {
            foreach (var emitterInfoID in emitterInfoIDs)
            {
                var emitterInfo = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.ParticleEmitterInfo>(emitterInfoID);
                Window.Status.WriteLine($"ParticleEmitterInfo.ID: {emitterInfoID:X8}");

                var frame = new AFrame();
                Player.PhysicsObj.create_particle_emitter(emitterInfoID, 0, frame, 0);
            }
        }

        public void InitEmitter(uint scriptID, float mod = 0.0f)
        {
            if (scriptID >> 24 == 0x32)
            {
                InitEmitter(new List<uint>() { scriptID }, mod);
                return;
            }

            var emitterInfoIDs = GetEmitterInfoIDs(scriptID, mod);
            InitEmitter(emitterInfoIDs, mod);
        }

        public void InitEmitter(uint pEffectTableID, PlayScript playScript, float mod = 0.0f)
        {
            var emitterInfoIDs = GetEmitterInfoIDs(pEffectTableID, playScript, mod);
            InitEmitter(emitterInfoIDs, mod);
        }

        protected override void Update(GameTime time)
        {
            // every update we can now query the keyboard & mouse for our WpfGame
            var mouseState = _mouse.GetState();
            var keyboardState = _keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.C) && !PrevKeyboardState.IsKeyDown(Keys.C))
            {
                // cancel all emitters in progress
                Player.PhysicsObj.ParticleManager.ParticleTable.Clear();
            }

            if (Player != null)
                Player.Update(time);

            //if (Camera != null)
                //Camera.Update(time);
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(new Color(0, 0, 0));

            if (Render != null)
                Render.Draw();
        }
    }
}
