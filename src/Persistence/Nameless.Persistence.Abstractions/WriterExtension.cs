namespace Nameless.Persistence {

    public static class WriterExtension {

		#region Public Static Methods

		public static Task SaveAsync<TEntity>(this IWriter self, SaveInstruction<TEntity> instruction, CancellationToken cancellationToken = default) where TEntity : class {
			if (self == null) { return Task.CompletedTask; }

			return self.SaveAsync(
				instructions: SaveInstructionCollection<TEntity>.Create(instruction),
				cancellationToken: cancellationToken
			);
		}

		public static Task DeleteAsync<TEntity>(this IWriter self, DeleteInstruction<TEntity> instruction, CancellationToken cancellationToken = default) where TEntity : class {
			if (self == null) { return Task.CompletedTask; }

			return self.DeleteAsync(
				instructions: DeleteInstructionCollection<TEntity>.Create(instruction),
				cancellationToken: cancellationToken
			);
		}

		#endregion
	}
}