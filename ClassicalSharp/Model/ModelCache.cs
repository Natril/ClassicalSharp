﻿using System;
using System.Collections.Generic;
using System.IO;
using ClassicalSharp.GraphicsAPI;

namespace ClassicalSharp.Model {

	public class ModelCache : IDisposable {
		
		Game game;
		IGraphicsApi api;
		public ModelCache( Game window ) {
			this.game = window;
			api = game.Graphics;		
		}
		
		public void InitCache() {
			vertices = new VertexPos3fTex2fCol4b[384];
			vb = game.Graphics.CreateDynamicVb( VertexFormat.Pos3fTex2fCol4b, vertices.Length );
			cache["humanoid"] = new PlayerModel( game );
		}
		
		internal int vb;
		internal VertexPos3fTex2fCol4b[] vertices;
		Dictionary<string, IModel> cache = new Dictionary<string, IModel>();
		
		public IModel GetModel( string modelName ) {
			IModel model;
			byte blockId;
			if( Byte.TryParse( modelName, out blockId ) ) {
				modelName = "block";
				if( blockId == 0 || blockId > BlockInfo.MaxDefinedBlock )
					return cache["humanoid"];
			}
			
			if( !cache.TryGetValue( modelName, out model ) ) {
				try {
					model = InitModel( modelName );
				} catch( FileNotFoundException ) {
					model = null;
					Utils.LogWarning( modelName + " not found, falling back to human default." );
				}
				if( model == null ) 
					model = cache["humanoid"]; // fallback to default
				cache[modelName] = model;
			}
			return model;
		}
		
		IModel InitModel( string modelName ) {
			if( modelName == "chicken" ) {
				return new ChickenModel( game );
			} else if( modelName == "creeper" ) {
				return new CreeperModel( game );
			} else if( modelName == "pig" ) {
				return new PigModel( game );
			} else if( modelName == "sheep" ) {
				return new SheepModel( game );
			} else if( modelName == "skeleton" ) {
				return new SkeletonModel( game );
			} else if( modelName == "spider" ) {
				return new SpiderModel( game );
			} else if( modelName == "zombie" ) {
				return new ZombieModel( game );
			} else if( modelName == "block" ) {
				return new BlockModel( game );
			}
			return null;
		}
		
		public void Dispose() {
			foreach( var entry in cache ) {
				entry.Value.Dispose();
			}
			game.Graphics.DeleteDynamicVb( vb );
		}
	}
}