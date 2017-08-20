﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Gui.Widgets;
using OpenTK.Input;

namespace ClassicalSharp.Gui.Screens {
	
	public sealed class TexIdsOverlay : Overlay {
		
		TextAtlas idAtlas;
		public TexIdsOverlay(Game game) : base(game) { }
		const int verticesCount = TerrainAtlas2D.TilesPerRow * TerrainAtlas2D.RowsCount * 4;
		static VertexP3fT2fC4b[] vertices;
		int dynamicVb;
		
		public override void Init() {
			base.Init();
			idAtlas = new TextAtlas(game, 16);
			idAtlas.Pack("0123456789", regularFont, "f");

			if (vertices == null) {
				vertices = new VertexP3fT2fC4b[verticesCount];
			}
			ContextRecreated();
		}
		
		const int tileSize = 45, textOffset = 3;
		int xOffset, yOffset;
		
		public override void Render(double delta) {
			RenderMenuBounds();
			gfx.Texturing = true;
			gfx.SetBatchFormat(VertexFormat.P3fT2fC4b);
			RenderWidgets(widgets, delta);
			RenderTerrain();			
			RenderTextOverlay();		
			gfx.Texturing = false;
		}
		
		protected override void ContextLost() {
			base.ContextLost();
			gfx.DeleteVb(ref dynamicVb);
			idAtlas.Dispose();
		}
		
		protected override void ContextRecreated() {
			base.ContextRecreated();
			dynamicVb = gfx.CreateDynamicVb(VertexFormat.P3fT2fC4b, verticesCount);
			idAtlas = new TextAtlas(game, 16);
			idAtlas.Pack("0123456789", regularFont, "f");
		}
		
		void RenderTerrain() {
			int elementsPerAtlas = game.TerrainAtlas1D.elementsPerAtlas1D;			
			for (int i = 0; i < TerrainAtlas2D.TilesPerRow * TerrainAtlas2D.RowsCount;) {
				int index = 0, texIdx = i / elementsPerAtlas, ignored;
				
				for (int j = 0; j < elementsPerAtlas; j++) {
					TextureRec rec = game.TerrainAtlas1D.GetTexRec(i + j, 1, out ignored);
					int x = (i + j) % TerrainAtlas2D.TilesPerRow;
					int y = (i + j) / TerrainAtlas2D.TilesPerRow;
					
					Texture tex = new Texture(0, xOffset + x * tileSize, yOffset + y * tileSize, 
					                          tileSize, tileSize, rec);
					IGraphicsApi.Make2DQuad(ref tex, FastColour.WhitePacked, vertices, ref index);
				}
				i += elementsPerAtlas;
				
				gfx.BindTexture(game.TerrainAtlas1D.TexIds[texIdx]);
				gfx.UpdateDynamicVb_IndexedTris(dynamicVb, vertices, index);
			}
		}
		
		void RenderTextOverlay() {
			int index = 0;
			idAtlas.tex.Y = (short)(yOffset + (tileSize - idAtlas.tex.Height));
			
			for (int y = 0; y < 4; y++) {
				for (int x = 0; x < TerrainAtlas2D.TilesPerRow; x++) {
					idAtlas.curX = xOffset + tileSize * x + textOffset;
					idAtlas.AddInt(x + (y * 16), vertices, ref index);
				}
				idAtlas.tex.Y += tileSize;
			}

			gfx.BindTexture(idAtlas.tex.ID);
			gfx.UpdateDynamicVb_IndexedTris(dynamicVb, vertices, index);
		}
		
		public override bool HandlesKeyDown(Key key) {
			if (key == Key.F10 || key == game.Input.Keys[KeyBind.PauseOrExit]) {
				Dispose();
				CloseOverlay();
			}
			return true;
		}

		public override void RedrawText() { }
		
		public override void MakeButtons() {
			xOffset = (game.Width / 2)  - (tileSize * TerrainAtlas2D.TilesPerRow) / 2;
			yOffset = (game.Height / 2) - (tileSize * TerrainAtlas2D.RowsCount)   / 2;
			
			widgets = new Widget[1];
			widgets[0] = TextWidget.Create(game, "Texture ID reference sheet", titleFont)
				.SetLocation(Anchor.Centre, Anchor.LeftOrTop, 0, yOffset - 30);
		}
	}
}