# ParticleViewer

A particle viewer for Asheron's Call

Finds particle emitters from Setups, PETables, physics scripts, or directly from emitter infos

1. The repo includes binaries in the bin/x64/Debug folder. The source is also included, buildable in VS 2017
2. Navigate to File/Open from the menu at the top, and select the folder with your AC client .dat files
3. Select the tab for how to browse for particle emitters:
  - Setup files (0x2) which contain DefaultScripts (0x33)
  - Physics effect tables (0x34) which contain lists of PlayScripts
  - Physics scripts directly (0x33)
  - Emitter info IDs directly (0x32)
4. Click on the IDs from the lists to view the particle emitters

To stop a particle simulation in progress, click the viewer window and then press the 'C' key
